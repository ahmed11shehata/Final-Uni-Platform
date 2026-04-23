using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Admin_Module;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/final-grade")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class AdminFinalGradeController : ControllerBase
    {
        private readonly IUnitOfWork            _unitOfWork;
        private readonly UserManager<User>      _userManager;
        private readonly INotificationService   _notifications;

        public AdminFinalGradeController(
            IUnitOfWork          unitOfWork,
            UserManager<User>    userManager,
            INotificationService notifications)
        {
            _unitOfWork    = unitOfWork;
            _userManager   = userManager;
            _notifications = notifications;
        }

        // ══════════════════════════════════════════════════════════════════
        //  GET /api/admin/final-grade/student/{studentCode}
        //  Lookup student by Academic_Code (or GUID) and return every
        //  currently-registered course with its final-grade status.
        // ══════════════════════════════════════════════════════════════════
        [HttpGet("student/{studentCode}")]
        public async Task<IActionResult> GetStudentGrades(string studentCode)
        {
            // Resolve student: try GUID first, fall back to academic code
            User? student = await _userManager.FindByIdAsync(studentCode);
            if (student == null)
            {
                var all = _userManager.Users
                    .Where(u => u.Academic_Code == studentCode)
                    .ToList();
                student = all.FirstOrDefault();
            }

            if (student == null)
                return NotFound(new { success = false, error = new { message = "Student not found." } });

            // Active registrations only (Approved status; dropped registrations are deleted)
            var regs = await _unitOfWork.Registrations.GetByUserIdAsync(student.Id);
            var active = (regs ?? Enumerable.Empty<Registration>())
                .Where(r => r.Status == RegistrationStatus.Approved)
                .ToList();

            // Pre-load quizzes and assignments for relevant courses
            var courseIds = active.Select(r => r.CourseId).Distinct().ToList();

            var courseDtos = new List<AdminFinalGradeCourseDto>();

            foreach (var reg in active)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(reg.CourseId);
                if (course == null) continue;

                // ── Midterm ──
                var midterm  = await _unitOfWork.MidtermGrades.GetAsync(student.Id, reg.CourseId);
                var midGrade = midterm?.Grade ?? 0;
                var midMax   = midterm?.Max   ?? 0;

                // ── Quizzes ──
                decimal quizScore = 0;
                var quizzes = (await _unitOfWork.Quizzes.GetQuizzesByCourseId(reg.CourseId)).ToList();
                foreach (var q in quizzes)
                {
                    var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, student.Id);
                    if (attempt != null) quizScore += attempt.Score;
                }

                // ── Assignments ──
                decimal asnScore = 0;
                var assignments = (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(reg.CourseId)).ToList();
                foreach (var a in assignments)
                {
                    var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, student.Id);
                    if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                        asnScore += sub.Grade ?? 0;
                }

                // ── Final grade record ──
                var fg     = await _unitOfWork.FinalGrades.GetAsync(student.Id, reg.CourseId);
                var bonus  = fg?.Bonus ?? 0;
                var cwTotal = Math.Min(40m, midGrade + quizScore + asnScore + bonus);
                var total   = fg != null ? (decimal?)(cwTotal + fg.FinalScore) : null;
                var letter  = total.HasValue ? GradeToLetter((int)Math.Round(total.Value)) : null;

                courseDtos.Add(new AdminFinalGradeCourseDto
                {
                    CourseId        = course.Id,
                    CourseCode      = course.Code,
                    CourseName      = course.Name,
                    MidtermGrade    = midGrade,
                    MidtermMax      = midMax,
                    QuizScore       = Math.Round(quizScore, 1),
                    AssignmentScore = Math.Round(asnScore, 1),
                    Bonus           = bonus,
                    CourseworkTotal = Math.Round(cwTotal, 1),
                    FinalScore      = fg?.FinalScore,
                    Total           = total.HasValue ? Math.Round(total.Value, 1) : null,
                    LetterGrade     = letter,
                    Assigned        = fg != null,
                    Published       = fg?.Published ?? false,
                });
            }

            return Ok(new
            {
                success = true,
                data = new AdminFinalGradeStudentDto
                {
                    StudentId   = student.Id,
                    StudentName = student.DisplayName,
                    StudentCode = student.Academic_Code,
                    Courses     = courseDtos,
                }
            });
        }

        // ══════════════════════════════════════════════════════════════════
        //  POST /api/admin/final-grade/publish/{studentId}
        //  Publish all assigned final grades for this student so students
        //  can see them.  Only grades that are fully assigned are published;
        //  unassigned courses are silently skipped.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("publish/{studentId}")]
        public async Task<IActionResult> PublishGrades(
            string studentId,
            [FromBody] AdminPublishFinalGradeDto? dto)
        {
            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
                return NotFound(new { success = false, error = new { message = "Student not found." } });

            var regs = await _unitOfWork.Registrations.GetByUserIdAsync(studentId);
            var active = (regs ?? Enumerable.Empty<Registration>())
                .Where(r => r.Status == RegistrationStatus.Approved)
                .ToList();

            // Filter to specific courses if requested
            if (dto?.CourseIds?.Count > 0)
                active = active.Where(r => dto.CourseIds.Contains(r.CourseId)).ToList();

            int publishedCount = 0;
            int skippedCount   = 0;

            foreach (var reg in active)
            {
                var fg = await _unitOfWork.FinalGrades.GetAsync(studentId, reg.CourseId);
                if (fg == null) { skippedCount++; continue; }

                if (!fg.Published)
                {
                    fg.Published = true;
                    await _unitOfWork.FinalGrades.UpdateAsync(fg);
                    publishedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    publishedCount,
                    skippedCount,
                    message = publishedCount > 0
                        ? $"{publishedCount} grade(s) published to student."
                        : "No new grades were published (all already published or unassigned)."
                }
            });
        }

        // ══════════════════════════════════════════════════════════════════
        //  POST /api/admin/final-grade/notify/{studentId}/{courseId}
        //  Send a warning notification to every instructor assigned to this
        //  course, alerting them that the student's final grade is missing.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("notify/{studentId}/{courseId:int}")]
        public async Task<IActionResult> NotifyInstructor(string studentId, int courseId)
        {
            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
                return NotFound(new { success = false, error = new { message = "Student not found." } });

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return NotFound(new { success = false, error = new { message = "Course not found." } });

            // Get all instructors assigned to this course
            var assignments = await _unitOfWork.RegistrationCourseInstructors.GetByCourseAsync(courseId);
            if (assignments == null || !assignments.Any())
                return BadRequest(new { success = false, error = new { message = "No instructors are assigned to this course." } });

            var notifs = new List<Notification>();
            foreach (var asgn in assignments)
            {
                var instructor = await _userManager.FindByIdAsync(asgn.InstructorId);
                if (instructor == null) continue;

                notifs.Add(new Notification
                {
                    UserId          = asgn.InstructorId,
                    Type            = "final_grade_warning",
                    Title           = "⚠️ Final Grade Not Assigned",
                    Body            = $"Student {student.DisplayName} ({student.Academic_Code}) is missing a final grade in {course.Name}. Please enter the grade as soon as possible.",
                    CourseId        = courseId,
                    CourseName      = course.Name,
                    StudentName     = student.DisplayName,
                    StudentCode     = student.Academic_Code,
                    TargetStudentId = student.Id,
                    IsRead          = false,
                });
            }

            if (notifs.Count > 0)
                await _notifications.SendManyAsync(notifs);

            return Ok(new
            {
                success = true,
                data = new
                {
                    notified    = notifs.Count,
                    courseName  = course.Name,
                    studentName = student.DisplayName,
                }
            });
        }

        private static string GradeToLetter(int total) => total switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _     => "F",
        };
    }
}
