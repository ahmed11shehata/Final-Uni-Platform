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
using Microsoft.EntityFrameworkCore;
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

        private const string ST_PROGRESS      = "progress";
        private const string ST_NOT_COMPLETED = "not_completed";
        private const string ST_COMPLETED     = "completed";

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

            // Active registrations: Pending (student self-registered) + Approved (admin force-added).
            // Must mirror StudentRegistrationService which treats both statuses as "registered".
            var regs = await _unitOfWork.Registrations.GetByUserIdAsync(student.Id);
            var active = (regs ?? Enumerable.Empty<Registration>())
                .Where(r => (r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending) && !r.IsEquivalency)
                .ToList();

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
                // AdminFinalTotal (set via Academic Setup) takes precedence over the computed total
                var total = fg != null
                    ? (fg.AdminFinalTotal.HasValue
                        ? (decimal?)fg.AdminFinalTotal.Value
                        : (decimal?)(cwTotal + fg.FinalScore))
                    : null;
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
        //  GET /api/admin/final-grade/students
        //  List ALL platform student accounts with their classification.
        //  Students with no current-term courses appear in Progress (count=0).
        //  Students never explicitly classified default to Progress.
        // ══════════════════════════════════════════════════════════════════
        [HttpGet("students")]
        public async Task<IActionResult> GetClassificationList()
        {
            // Source of truth: every Student-role account on the platform.
            var allStudents = await _userManager.GetUsersInRoleAsync("Student");
            if (!allStudents.Any())
                return Ok(new { success = true, data = new AdminFinalGradeReviewListDto() });

            // Current-term registration counts per student (best-effort; 0 when no active term).
            var regCountByStudent = new Dictionary<string, int>();
            var reviewByStudent   = new Dictionary<string, string>();

            var (studyYearId, semesterId) = await ResolveCurrentTermAsync();
            if (studyYearId > 0 && semesterId > 0)
            {
                var allRegs = await _unitOfWork.Registrations.GetAllAsync(
                    studyYearId: studyYearId, semesterId: semesterId);
                var activeRegs = allRegs
                    .Where(r => (r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending) && !r.IsEquivalency)
                    .ToList();
                regCountByStudent = activeRegs
                    .GroupBy(r => r.UserId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var reviews = await _unitOfWork.FinalGradeReviews.GetByTermAsync(studyYearId, semesterId);
                reviewByStudent = reviews.ToDictionary(r => r.StudentId, r => r.Status);
            }

            var dto = new AdminFinalGradeReviewListDto();
            foreach (var u in allStudents)
            {
                reviewByStudent.TryGetValue(u.Id, out var rawStatus);
                var status = NormalizeStatus(rawStatus);

                regCountByStudent.TryGetValue(u.Id, out var courseCount);

                var item = new AdminFinalGradeReviewStudentDto
                {
                    StudentId         = u.Id,
                    StudentName       = u.DisplayName ?? "",
                    StudentCode       = u.Academic_Code ?? "",
                    AcademicYear      = LevelToAcademicYearLabel(u.Level),
                    RegisteredCourses = courseCount,
                    Status            = status,
                };

                switch (status)
                {
                    case ST_COMPLETED:     dto.Completed.Add(item);    break;
                    case ST_NOT_COMPLETED: dto.NotCompleted.Add(item); break;
                    default:               dto.Progress.Add(item);     break;
                }
            }

            dto.Total         = dto.Progress.Count + dto.NotCompleted.Count + dto.Completed.Count;
            dto.CanPublishAll = dto.Total > 0 && dto.Progress.Count == 0 && dto.NotCompleted.Count == 0;

            return Ok(new { success = true, data = dto });
        }

        // ══════════════════════════════════════════════════════════════════
        //  POST /api/admin/final-grade/classify/{studentId}
        //  Admin manually marks a student as progress/not_completed/completed.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("classify/{studentId}")]
        public async Task<IActionResult> ClassifyStudent(
            string studentId,
            [FromBody] AdminClassifyStudentDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, error = new { message = "Body required." } });

            var status = NormalizeStatus(dto.Status);

            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
                return NotFound(new { success = false, error = new { message = "Student not found." } });

            var (studyYearId, semesterId) = await ResolveCurrentTermAsync();
            if (studyYearId == 0 || semesterId == 0)
                return BadRequest(new { success = false, error = new { message = "No active current term." } });

            var existing = await _unitOfWork.FinalGradeReviews.GetAsync(studentId, studyYearId, semesterId);
            if (existing == null)
            {
                await _unitOfWork.FinalGradeReviews.AddAsync(new FinalGradeReview
                {
                    StudentId   = studentId,
                    StudyYearId = studyYearId,
                    SemesterId  = semesterId,
                    Status      = status,
                    UpdatedAt   = DateTime.UtcNow,
                });
            }
            else
            {
                existing.Status    = status;
                existing.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.FinalGradeReviews.UpdateAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true, data = new { studentId, status } });
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
                .Where(r => (r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending) && !r.IsEquivalency)
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
        //  POST /api/admin/final-grade/publish-all
        //  Global publish: publishes all assigned grades for EVERY student
        //  account on the platform. Blocked when any student account is still
        //  in Progress or Not Completed (mirrors CanPublishAll from the list).
        //
        //  Registration source: GetByUserIdAsync (all-term, same as
        //  GetStudentGrades and PublishGrades) so that courses whose
        //  StudyYearId/SemesterId differs from the resolved current-term IDs
        //  are NOT silently skipped — they get published together with the rest.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("publish-all")]
        public async Task<IActionResult> PublishAll()
        {
            var (studyYearId, semesterId) = await ResolveCurrentTermAsync();
            if (studyYearId == 0 || semesterId == 0)
                return BadRequest(new { success = false, error = new { code = "NO_TERM", message = "No active current term." } });

            // ── Gate: ALL platform student accounts must be Completed ──
            // Mirrors CanPublishAll = (Progress.Count == 0 && NotCompleted.Count == 0)
            // in GetClassificationList so the frontend and backend agree on the same set.
            var allStudents = (await _userManager.GetUsersInRoleAsync("Student")).ToList();
            if (!allStudents.Any())
                return BadRequest(new { success = false, error = new { code = "NO_STUDENTS", message = "No student accounts found on the platform." } });

            var reviews = await _unitOfWork.FinalGradeReviews.GetByTermAsync(studyYearId, semesterId);
            var statusByStudent = reviews.ToDictionary(r => r.StudentId, r => NormalizeStatus(r.Status));

            var blocking = allStudents
                .Count(u => !statusByStudent.TryGetValue(u.Id, out var st) || st != ST_COMPLETED);

            if (blocking > 0)
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code    = "REVIEW_INCOMPLETE",
                        message = $"{blocking} student(s) are still in Progress or Not Completed. Publish is disabled until every student is marked Completed.",
                    }
                });

            // ── Publish: all active registrations for each student ──
            // Use GetByUserIdAsync (no term filter) to match GetStudentGrades and
            // per-student PublishGrades — prevents partial publish when registration
            // term IDs differ from the resolved current-term IDs.
            int publishedCount  = 0;
            int skippedCount    = 0;
            int studentsTouched = 0;

            foreach (var user in allStudents)
            {
                var regs = await _unitOfWork.Registrations.GetByUserIdAsync(user.Id);
                var active = (regs ?? Enumerable.Empty<Registration>())
                    .Where(r => (r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending)
                                && !r.IsEquivalency)
                    .ToList();

                bool touched = false;
                foreach (var reg in active)
                {
                    var fg = await _unitOfWork.FinalGrades.GetAsync(user.Id, reg.CourseId);
                    if (fg == null) { skippedCount++; continue; }
                    if (fg.Published) continue;

                    fg.Published = true;
                    await _unitOfWork.FinalGrades.UpdateAsync(fg);
                    publishedCount++;
                    touched = true;
                }
                if (touched) studentsTouched++;
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    publishedCount,
                    skippedCount,
                    studentsTouched,
                    message = $"Published {publishedCount} grade(s) across {studentsTouched} student(s)."
                }
            });
        }

        // ══════════════════════════════════════════════════════════════════
        //  POST /api/admin/final-grade/notify/{studentId}/{courseId}
        //  Send a warning notification to every instructor assigned to this
        //  course. Enforces a once-per-day limit per (instructor, student,
        //  course) — clicking the warning button more than once a day is a
        //  no-op rather than spamming the instructor.
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

            var assignments = await _unitOfWork.RegistrationCourseInstructors.GetByCourseAsync(courseId);
            if (assignments == null || !assignments.Any())
                return BadRequest(new { success = false, error = new { message = "No instructors are assigned to this course." } });

            var notifs      = new List<Notification>();
            int suppressed  = 0;
            var todayUtc    = DateTime.UtcNow.Date;

            foreach (var asgn in assignments)
            {
                var instructor = await _userManager.FindByIdAsync(asgn.InstructorId);
                if (instructor == null) continue;

                // ── Once-per-day guard: same (instructor, student, course, type, day) ──
                var recent = await _unitOfWork.Notifications.GetForUserAsync(asgn.InstructorId);
                bool sentToday = recent.Any(n =>
                    n.Type == "final_grade_warning" &&
                    n.CourseId == courseId &&
                    n.TargetStudentId == student.Id &&
                    n.CreatedAt >= todayUtc);

                if (sentToday) { suppressed++; continue; }

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
                    suppressed,
                    courseName  = course.Name,
                    studentName = student.DisplayName,
                    message = notifs.Count > 0
                        ? $"Notified {notifs.Count} instructor(s)."
                                  + (suppressed > 0 ? $" Skipped {suppressed} (already warned today)." : "")
                        : "All instructors were already warned today for this case.",
                }
            });
        }

        // ══════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════

        private async Task<(int studyYearId, int semesterId)> ResolveCurrentTermAsync()
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();

            StudyYear? studyYear = null;
            if (settings != null && !string.IsNullOrWhiteSpace(settings.AcademicYear))
            {
                var parts = settings.AcademicYear.Split('/', '-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out var sYr) &&
                    int.TryParse(parts[1].Trim(), out var eYr))
                {
                    var all = await _unitOfWork.StudyYears.GetAllAsync();
                    studyYear = all.FirstOrDefault(y => y.StartYear == sYr && y.EndYear == eYr);
                }
            }
            studyYear ??= await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (studyYear == null) return (0, 0);

            Semester? semester = null;
            if (settings != null && !string.IsNullOrWhiteSpace(settings.Semester))
            {
                var semKey  = settings.Semester.Trim().ToLowerInvariant();
                var allSems = await _unitOfWork.Semesters.GetByStudyYearIdAsync(studyYear.Id);
                semester = semKey switch
                {
                    "first" or "first semester" or "semester 1" or "1" =>
                        allSems.FirstOrDefault(s => s.Title == SemesterEnum.First_Semester),
                    "second" or "second semester" or "semester 2" or "2" =>
                        allSems.FirstOrDefault(s => s.Title == SemesterEnum.Second_Semester),
                    "summer" or "summer semester" or "3" =>
                        allSems.FirstOrDefault(s => s.Title == SemesterEnum.Summer),
                    _ => allSems.FirstOrDefault()
                };
            }
            semester ??= await _unitOfWork.Semesters.GetActiveSemesterByStudyYearIdAsync(studyYear.Id);
            if (semester == null) return (studyYear.Id, 0);

            return (studyYear.Id, semester.Id);
        }

        private static string NormalizeStatus(string? status) => (status ?? "").Trim().ToLowerInvariant() switch
        {
            "completed"     => ST_COMPLETED,
            "not_completed" or "notcompleted" or "not-completed" => ST_NOT_COMPLETED,
            _               => ST_PROGRESS,
        };

        private static string LevelToAcademicYearLabel(Levels? level) => level switch
        {
            Levels.Preparatory_Year => "First",
            Levels.First_Year       => "First",
            Levels.Second_Year      => "Second",
            Levels.Third_Year       => "Third",
            Levels.Fourth_Year      => "Fourth",
            Levels.Graduate         => "Fourth",
            _                       => "First",
        };

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
