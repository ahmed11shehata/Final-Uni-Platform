using System.Security.Claims;
using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Instructor_Module;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/instructor")]
    [Authorize(Roles = "Instructor")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class InstructorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notifications;

        private static readonly string[] Colors = {
            "#4F46E5","#059669","#D97706","#DC2626","#7C3AED",
            "#0891B2","#6366f1","#3b82f6","#22c55e","#e05c8a","#f97316","#14b8a6"
        };
        private static readonly string[] Icons = {
            "📚","🔬","💡","🧠","⚙️","🌐","🖥️","🔭","📐","🧮","🎯","🔑"
        };

        public InstructorController(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            INotificationService notifications)
        {
            _unitOfWork    = unitOfWork;
            _userManager   = userManager;
            _notifications = notifications;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private static int StableIndex(string code) =>
            Math.Abs(code?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);

        /// <summary>Confirm caller is assigned to this course via Instructor Control.</summary>
        private async Task<bool> InstructorOwnsCourse(int courseId)
        {
            var list = await _unitOfWork.RegistrationCourseInstructors.GetByCourseAsync(courseId);
            return list.Any(x => x.InstructorId == CurrentUserId);
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses
        //  Returns all courses the logged-in instructor is assigned to.
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid))
                return Unauthorized();

            // All course assignments for this instructor
            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var assignments = new List<int>();
            foreach (var course in allCourses)
            {
                var instructors = await _unitOfWork.RegistrationCourseInstructors.GetByCourseAsync(course.Id);
                if (instructors.Any(x => x.InstructorId == uid))
                    assignments.Add(course.Id);
            }

            var result = new List<InstructorCourseDto>();
            foreach (var cid in assignments)
            {
                var course = allCourses.FirstOrDefault(c => c.Id == cid);
                if (course == null) continue;

                var idx = StableIndex(course.Code);
                // Count enrolled students
                var regs = await _unitOfWork.Registrations.GetByCourseIdAsync(cid);
                var studentCount = regs?.Count() ?? 0;

                result.Add(new InstructorCourseDto
                {
                    Id       = course.Id.ToString(),
                    Code     = course.Code,
                    Name     = course.Name,
                    Color    = Colors[idx % Colors.Length],
                    Icon     = Icons[idx % Icons.Length],
                    Students = studentCount,
                    Progress = 0,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/students
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/students")]
        public async Task<IActionResult> GetCourseStudents(int courseId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var regs = await _unitOfWork.Registrations.GetByCourseIdAsync(courseId);
            var result = new List<StudentInCourseDto>();

            foreach (var reg in regs ?? Enumerable.Empty<Registration>())
            {
                var user = await _userManager.FindByIdAsync(reg.UserId);
                if (user == null) continue;

                var idx = StableIndex(user.Email ?? user.Id);
                var color = Colors[idx % Colors.Length];
                var name  = user.DisplayName;
                var ini   = string.Concat(name.Split(' ').Take(2).Select(w => w.Length > 0 ? w[0].ToString() : ""));

                result.Add(new StudentInCourseDto
                {
                    Id       = user.Id,
                    Name     = name,
                    Email    = user.Email ?? "",
                    Color    = color,
                    Initials = ini,
                    Submissions = 0,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/assignments?courseId={id}
        // ══════════════════════════════════════════════════════════
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments([FromQuery] int? courseId)
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            IEnumerable<Assignment> assignments;

            if (courseId.HasValue)
            {
                if (!await InstructorOwnsCourse(courseId.Value))
                    return Forbid();

                assignments = await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseId.Value);
            }
            else
            {
                // All assignments across all instructor-owned courses
                var allCourses = await _unitOfWork.Courses.GetAllAsync();
                var owned = new List<int>();
                foreach (var c in allCourses)
                {
                    var ins = await _unitOfWork.RegistrationCourseInstructors.GetByCourseAsync(c.Id);
                    if (ins.Any(x => x.InstructorId == uid)) owned.Add(c.Id);
                }
                var all = new List<Assignment>();
                foreach (var cid in owned)
                    all.AddRange(await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(cid));
                assignments = all;
            }

            var now2 = DateTime.UtcNow;
            var result = assignments.Select(a => new InstructorAssignmentDto
            {
                Id             = a.Id.ToString(),
                Title          = a.Title,
                CourseId       = a.CourseId.ToString(),
                CourseCode     = a.Course?.Code ?? "",
                CourseName     = a.Course?.Name ?? "",
                Deadline       = a.Deadline.ToString("yyyy-MM-dd HH:mm"),
                ReleaseDate    = a.ReleaseDate?.ToString("yyyy-MM-dd HH:mm"),
                MaxGrade       = a.Points,
                Description    = a.Description ?? "",
                AllowedFormats = new List<string>(),
                Status         = now2 > a.Deadline ? "closed"
                                 : (a.ReleaseDate.HasValue && now2 < a.ReleaseDate.Value) ? "scheduled"
                                 : "open",
                SubmissionsCount = a.Submissions?.Count ?? 0,
                PendingCount   = a.Submissions?.Count(s => s.Status == "Pending") ?? 0,
                AttachmentUrl  = string.IsNullOrWhiteSpace(a.FileUrl) ? null : a.FileUrl,
            }).ToList();

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  POST /api/instructor/assignments  (multipart/form-data)
        //  Accepts an optional instructor attachment file.
        // ══════════════════════════════════════════════════════════
        [HttpPost("assignments")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateAssignment(
            [FromForm] CreateInstructorAssignmentDto dto,
            IFormFile? attachmentFile,
            [FromServices] ILocalFileService fileService,
            [FromServices] ICourseworkBudgetService budget)
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { success = false, error = new { message = "Title is required." } });

            // Assignment max grade is REQUIRED — must be selected, must be 1..5.
            if (dto.MaxGrade < 1 || dto.MaxGrade > 5)
                return BadRequest(new { success = false, error = new {
                    code = "MAX_GRADE_REQUIRED",
                    message = "Assignment max grade is required and must be between 1 and 5." } });

            // Resolve course by code OR by ID
            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var course = allCourses.FirstOrDefault(c =>
                c.Code.Equals(dto.CourseCode, StringComparison.OrdinalIgnoreCase));

            if (course == null)
                return BadRequest(new { success = false, error = new { message = "Course not found." } });

            if (!await InstructorOwnsCourse(course.Id))
                return Forbid();

            // Coursework budget check (assignments + quizzes + midterm <= 40).
            var bv = await budget.ValidateAddAssignmentAsync(course.Id, dto.MaxGrade);
            if (!bv.Ok)
                return BadRequest(new { success = false, error = new {
                    code = "COURSEWORK_BUDGET_EXCEEDED",
                    message = bv.Message,
                    used = bv.Used, remaining = bv.Remaining, requested = bv.Requested } });

            if (!DateTime.TryParse(dto.Deadline, out var deadline))
                return BadRequest(new { success = false, error = new { message = "Invalid deadline format." } });

            DateTime? releaseDate = null;
            if (!string.IsNullOrWhiteSpace(dto.ReleaseDate) && DateTime.TryParse(dto.ReleaseDate, out var rd))
                releaseDate = rd.ToUniversalTime();

            // Upload optional instructor attachment before persisting the assignment
            string? attachmentUrl = null;
            if (attachmentFile is not null)
            {
                var fileId = Guid.NewGuid().ToString();
                attachmentUrl = await fileService.UploadAssignmentFileAsync(
                    attachmentFile, fileId, course.Id, HttpContext.RequestAborted);
            }

            var assignment = new Assignment
            {
                Title           = dto.Title,
                Description     = dto.Description ?? "",
                Points          = dto.MaxGrade,   // already validated to be 1..5 above
                Deadline        = deadline,
                ReleaseDate     = releaseDate,
                CourseId        = course.Id,
                CreatedByUserId = uid,
                FileUrl         = attachmentUrl ?? string.Empty,
            };

            await _unitOfWork.Assignments.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            // ── Notify all enrolled students ──
            var instructorUser = await _userManager.FindByIdAsync(uid);
            var instructorName = instructorUser?.DisplayName ?? "Instructor";
            var regsForAsn = await _unitOfWork.Registrations.GetByCourseIdAsync(course.Id);
            var asnNotifs = (regsForAsn ?? Enumerable.Empty<Registration>())
                .Select(r => new Notification
                {
                    UserId          = r.UserId,
                    Type            = "assignment_published",
                    Title           = "New Assignment Posted 📋",
                    Body            = $"'{assignment.Title}' has been posted in {course.Name}.",
                    CourseId        = course.Id,
                    CourseName      = course.Name,
                    AssignmentId    = assignment.Id,
                    AssignmentTitle = assignment.Title,
                    InstructorName  = instructorName,
                    IsRead          = false,
                })
                .ToList();
            if (asnNotifs.Count > 0)
                await _notifications.SendManyAsync(asnNotifs);

            return StatusCode(201, new
            {
                success = true,
                data = new InstructorAssignmentDto
                {
                    Id            = assignment.Id.ToString(),
                    Title         = assignment.Title,
                    CourseId      = course.Id.ToString(),
                    CourseCode    = course.Code,
                    CourseName    = course.Name,
                    Deadline      = assignment.Deadline.ToString("yyyy-MM-dd HH:mm"),
                    ReleaseDate   = assignment.ReleaseDate?.ToString("yyyy-MM-dd HH:mm"),
                    MaxGrade      = assignment.Points,
                    Description   = assignment.Description,
                    Status        = "open",
                    AttachmentUrl = assignment.FileUrl,
                }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  PUT /api/instructor/assignments/{assignmentId}  (multipart/form-data)
        //  Supports replacing the attachment file and explicit clearing.
        //  Blocks Points changes once any submission has been graded.
        // ══════════════════════════════════════════════════════════
        [HttpPut("assignments/{assignmentId:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAssignment(
            int assignmentId,
            [FromForm] UpdateInstructorAssignmentDto dto,
            IFormFile? attachmentFile,
            [FromServices] ILocalFileService fileService)
        {
            if (dto == null) return BadRequest();

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound(new { success = false, error = new { message = "Assignment not found." } });

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            // Spec: assignment max grade is locked after creation regardless of grading state.
            // Other metadata (title, description, deadline, attachment) is still editable.
            if (dto.MaxGrade > 0 && dto.MaxGrade != assignment.Points)
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code    = "POINTS_LOCKED",
                        message = "Assignment max grade cannot be changed after the assignment is created. Other fields can still be updated."
                    }
                });

            if (!string.IsNullOrWhiteSpace(dto.Title))        assignment.Title       = dto.Title;
            if (dto.Description != null)                       assignment.Description = dto.Description;
            if (DateTime.TryParse(dto.Deadline, out var dl))   assignment.Deadline    = dl;
            if (!string.IsNullOrWhiteSpace(dto.ReleaseDate) && DateTime.TryParse(dto.ReleaseDate, out var rdu))
                assignment.ReleaseDate = rdu.ToUniversalTime();
            else if (dto.ClearReleaseDate)
                assignment.ReleaseDate = null;

            // ── Attachment handling ──
            // 1) New file provided → delete old file (if any) then upload new
            // 2) RemoveAttachment=true (no new file) → delete old file
            // 3) Otherwise leave as-is
            if (attachmentFile is not null)
            {
                if (!string.IsNullOrWhiteSpace(assignment.FileUrl))
                    await fileService.DeleteFileByUrlAsync(assignment.FileUrl);

                var fileId = Guid.NewGuid().ToString();
                var newUrl = await fileService.UploadAssignmentFileAsync(
                    attachmentFile, fileId, assignment.CourseId, HttpContext.RequestAborted);
                assignment.FileUrl = newUrl ?? string.Empty;
            }
            else if (dto.RemoveAttachment)
            {
                if (!string.IsNullOrWhiteSpace(assignment.FileUrl))
                    await fileService.DeleteFileByUrlAsync(assignment.FileUrl);
                assignment.FileUrl = string.Empty;
            }

            await _unitOfWork.Assignments.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, data = new InstructorAssignmentDto
            {
                Id            = assignment.Id.ToString(),
                Title         = assignment.Title,
                CourseId      = assignment.CourseId.ToString(),
                CourseCode    = assignment.Course?.Code ?? "",
                CourseName    = assignment.Course?.Name ?? "",
                Deadline      = assignment.Deadline.ToString("yyyy-MM-dd HH:mm"),
                ReleaseDate   = assignment.ReleaseDate?.ToString("yyyy-MM-dd HH:mm"),
                MaxGrade      = assignment.Points,
                Description   = assignment.Description ?? "",
                AttachmentUrl = string.IsNullOrWhiteSpace(assignment.FileUrl) ? null : assignment.FileUrl,
            }});
        }

        // ══════════════════════════════════════════════════════════
        //  DELETE /api/instructor/assignments/{assignmentId}
        //  Hard-deletes the assignment, all submissions, and their files.
        //  Coursework totals re-aggregate naturally on the next read.
        // ══════════════════════════════════════════════════════════
        [HttpDelete("assignments/{assignmentId:int}")]
        public async Task<IActionResult> DeleteAssignment(
            int assignmentId,
            [FromServices] ILocalFileService fileService)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound();

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            // Load every submission (incl. "Cleared") so files on disk are also removed.
            var allSubs = (assignment.Submissions ?? new List<AssignmentSubmission>()).ToList();
            foreach (var sub in allSubs)
            {
                if (!string.IsNullOrWhiteSpace(sub.FileUrl))
                    await fileService.DeleteFileByUrlAsync(sub.FileUrl);
                await _unitOfWork.Assignments.DeleteSubmissionAsync(sub.Id);
            }

            // Remove instructor attachment file
            if (!string.IsNullOrWhiteSpace(assignment.FileUrl))
                await fileService.DeleteFileByUrlAsync(assignment.FileUrl);

            await _unitOfWork.Assignments.DeleteAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = "Assignment deleted.", data = new { id = assignmentId, removedSubmissions = allSubs.Count } });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/assignments/{assignmentId}/submissions
        // ══════════════════════════════════════════════════════════
        [HttpGet("assignments/{assignmentId:int}/submissions")]
        public async Task<IActionResult> GetSubmissions(int assignmentId)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound();

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            var subs = await _unitOfWork.Assignments.GetSubmissions(assignmentId);

            var result = new List<SubmissionDto>();
            foreach (var s in subs)
            {
                var student = await _userManager.FindByIdAsync(s.StudentId);
                var name = student?.DisplayName ?? s.StudentId;

                // Extract file name from URL
                var fileName = string.IsNullOrEmpty(s.FileUrl)
                    ? "—"
                    : Path.GetFileName(s.FileUrl);

                result.Add(new SubmissionDto
                {
                    Id              = s.Id.ToString(),
                    AssignmentId    = assignment.Id.ToString(),
                    AssignmentTitle = assignment.Title,
                    CourseCode      = assignment.Course?.Code ?? "",
                    StudentId       = s.StudentId,
                    StudentName     = name,
                    SubmittedAt     = s.SubmittedAt.ToString("MMM dd · hh:mm tt"),
                    FileName        = fileName,
                    FileUrl         = s.FileUrl,
                    Status          = s.Status.ToLower(),
                    Grade           = s.Grade,
                    MaxGrade        = assignment.Points,
                    Feedback        = s.Feedback,
                    RejectionReason = s.RejectionReason,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  POST /api/instructor/assignments/{assignmentId}/submissions/{submissionId}/accept
        // ══════════════════════════════════════════════════════════
        [HttpPost("assignments/{assignmentId:int}/submissions/{submissionId:int}/accept")]
        public async Task<IActionResult> AcceptSubmission(int assignmentId, int submissionId)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound();

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            var submission = await _unitOfWork.Assignments.GetSubmissionByIdAsync(submissionId);
            if (submission == null || submission.AssignmentId != assignmentId)
                return NotFound(new { success = false, error = new { message = "Submission not found." } });

            if (submission.Status == "Rejected")
                return BadRequest(new { success = false, error = new { message = "Cannot accept a rejected submission." } });

            submission.Status = "Accepted";
            submission.Grade  = assignment.Points;
            submission.RejectionReason = null;
            await _unitOfWork.Assignments.UpdateSubmissionAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            // Notify student via service (persists + pushes SignalR)
            var course = await _unitOfWork.Courses.GetByIdAsync(assignment.CourseId);
            var acceptingInstructor = await _userManager.FindByIdAsync(CurrentUserId!);
            await _notifications.SendAsync(new Notification
            {
                UserId          = submission.StudentId,
                Type            = "grade_approved",
                Title           = "Assignment Accepted ✅",
                Body            = $"Your submission for '{assignment.Title}' has been accepted.",
                CourseId        = assignment.CourseId,
                CourseName      = course?.Name ?? "",
                AssignmentId    = assignment.Id,
                AssignmentTitle = assignment.Title,
                Grade           = assignment.Points,
                Max             = assignment.Points,
                InstructorName  = acceptingInstructor?.DisplayName ?? "Instructor",
                IsRead          = false,
            });

            return Ok(new { success = true, data = new { submissionId, status = "accepted", grade = assignment.Points } });
        }

        // ══════════════════════════════════════════════════════════
        //  POST /api/instructor/assignments/{assignmentId}/submissions/{submissionId}/reject
        // ══════════════════════════════════════════════════════════
        [HttpPost("assignments/{assignmentId:int}/submissions/{submissionId:int}/reject")]
        public async Task<IActionResult> RejectSubmission(
            int assignmentId, int submissionId, [FromBody] RejectSubmissionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { success = false, error = new { message = "Rejection reason is required." } });

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound();

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            var submission = await _unitOfWork.Assignments.GetSubmissionByIdAsync(submissionId);
            if (submission == null || submission.AssignmentId != assignmentId)
                return NotFound(new { success = false, error = new { message = "Submission not found." } });

            submission.Status          = "Rejected";
            submission.Grade           = 0;
            submission.RejectionReason = dto.Reason;
            await _unitOfWork.Assignments.UpdateSubmissionAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            // Notify student via service (persists + pushes SignalR)
            var course = await _unitOfWork.Courses.GetByIdAsync(assignment.CourseId);
            var rejectingInstructor = await _userManager.FindByIdAsync(CurrentUserId!);
            await _notifications.SendAsync(new Notification
            {
                UserId          = submission.StudentId,
                Type            = "grade_rejected",
                Title           = "Assignment Returned ❌",
                Body            = $"Your submission for '{assignment.Title}' was returned.",
                CourseId        = assignment.CourseId,
                CourseName      = course?.Name ?? "",
                AssignmentId    = assignment.Id,
                AssignmentTitle = assignment.Title,
                Grade           = 0,
                Max             = assignment.Points,
                RejectionReason = dto.Reason,
                InstructorName  = rejectingInstructor?.DisplayName ?? "Instructor",
                IsRead          = false,
            });

            return Ok(new { success = true, data = new { submissionId, status = "rejected", reason = dto.Reason } });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/quizzes
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/quizzes")]
        public async Task<IActionResult> GetCourseQuizzes(int courseId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var course  = await _unitOfWork.Courses.GetByIdAsync(courseId);
            var quizzes = await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseId);
            var now = DateTime.UtcNow;

            var result = new List<InstructorQuizDto>();
            foreach (var q in quizzes)
            {
                var attempts = await _unitOfWork.Quizzes.GetAttemptsByQuizIdAsync(q.Id);
                var attList  = attempts?.ToList() ?? new List<StudentQuizAttempt>();
                var qCount   = q.Questions?.Count ?? 0;

                string status = now < q.StartTime ? "upcoming"
                    : now <= q.EndTime ? "active"
                    : "ended";

                decimal? avg = attList.Count > 0
                    ? Math.Round((decimal)attList.Average(a => a.Score), 1)
                    : null;

                result.Add(new InstructorQuizDto
                {
                    Id          = q.Id.ToString(),
                    CourseId    = courseId.ToString(),
                    CourseCode  = course?.Code ?? "",
                    CourseName  = course?.Name ?? "",
                    Title       = q.Title,
                    Duration    = (int)(q.EndTime - q.StartTime).TotalMinutes,
                    Questions   = qCount,
                    Status      = status,
                    StartTime   = q.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    EndTime     = q.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Submissions = attList.Count,
                    AvgScore    = avg,
                    HasAttempts = attList.Count > 0,
                    // Quiz max = questions × points-per-question (matches student-facing scoring).
                    TotalPoints = qCount * (q.GradePerQuestion <= 0 ? 1 : q.GradePerQuestion),
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  PUT /api/instructor/courses/{courseId}/quizzes/{quizId}
        //  When attempts exist → only Title/StartTime/EndTime are mutable; questions are frozen.
        // ══════════════════════════════════════════════════════════
        [HttpPut("courses/{courseId:int}/quizzes/{quizId:int}")]
        public async Task<IActionResult> UpdateQuiz(
            int courseId, int quizId,
            [FromBody] UpdateInstructorQuizDto dto,
            [FromServices] ICourseworkBudgetService budget)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null || quiz.CourseId != courseId)
                return NotFound(new { success = false, error = new { code = "QUIZ_NOT_FOUND", message = "Quiz not found." } });

            if (dto == null) return BadRequest();

            bool hasAttempts = await _unitOfWork.Quizzes.HasAttemptsAsync(quizId);

            if (dto.Questions != null && dto.Questions.Count > 0 && hasAttempts)
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code = "QUESTIONS_LOCKED",
                        message = "Students have already attempted this quiz, so the question structure cannot be changed. Title and times can still be updated."
                    }
                });

            // GradePerQuestion can only change while no student has attempted the quiz.
            decimal existingGradePerQ = quiz.GradePerQuestion <= 0m ? 1m : quiz.GradePerQuestion;
            decimal newGradePerQ = existingGradePerQ;
            if (dto.GradePerQ.HasValue && dto.GradePerQ.Value != existingGradePerQ)
            {
                if (hasAttempts)
                    return BadRequest(new { success = false, error = new {
                        code = "POINTS_LOCKED",
                        message = "Points per question cannot be changed after students have attempted this quiz." } });
                if (dto.GradePerQ.Value < 0.5m || dto.GradePerQ.Value > 10m)
                    return BadRequest(new { success = false, error = new { message = "Points per question must be between 0.5 and 10." } });
                newGradePerQ = dto.GradePerQ.Value;
            }

            // If question structure or points-per-question changed, validate against the budget.
            int existingQuestionCount    = quiz.Questions?.Count ?? 0;
            decimal existingTotalPoints  = existingQuestionCount * existingGradePerQ;
            bool questionsChanging       = dto.Questions != null && dto.Questions.Count > 0 && !hasAttempts;
            bool pointsChanging          = newGradePerQ != existingGradePerQ;

            if (questionsChanging || pointsChanging)
            {
                int newQuestionCount = questionsChanging
                    ? dto.Questions!.Count(q => !string.IsNullOrWhiteSpace(q.Text))
                    : existingQuestionCount;
                decimal newTotalPoints = newQuestionCount * newGradePerQ;
                var qv = await budget.ValidateUpdateQuizAsync(courseId, existingTotalPoints, newTotalPoints);
                if (!qv.Ok)
                    return BadRequest(new { success = false, error = new {
                        code = "COURSEWORK_BUDGET_EXCEEDED",
                        message = qv.Message,
                        used = qv.Used, remaining = qv.Remaining, requested = qv.Requested } });
            }

            // ── Metadata ──
            if (!string.IsNullOrWhiteSpace(dto.Title)) quiz.Title = dto.Title;
            if (dto.StartTime.HasValue) quiz.StartTime = dto.StartTime.Value.ToUniversalTime();
            if (dto.EndTime.HasValue)   quiz.EndTime   = dto.EndTime.Value.ToUniversalTime();
            quiz.GradePerQuestion = newGradePerQ;
            if (quiz.EndTime <= quiz.StartTime)
                return BadRequest(new { success = false, error = new { message = "EndTime must be after StartTime." } });

            // ── Question structure replacement (only when no attempts) ──
            // Wipe existing questions/options first so child rows are removed cleanly,
            // then attach the new question set.
            if (dto.Questions != null && dto.Questions.Count > 0 && !hasAttempts)
            {
                foreach (var oq in quiz.Questions.ToList())
                {
                    foreach (var opt in oq.Options.ToList())
                        oq.Options.Remove(opt);
                    quiz.Questions.Remove(oq);
                }

                foreach (var qDto in dto.Questions)
                {
                    if (string.IsNullOrWhiteSpace(qDto.Text)) continue;
                    var question = new QuizQuestion
                    {
                        QuestionText = qDto.Text,
                        Type         = AYA_UIS.Core.Domain.Enums.QuestionType.MCQ,
                        Options      = new List<QuizOption>(),
                    };
                    for (int i = 0; i < qDto.Answers.Count; i++)
                    {
                        question.Options.Add(new QuizOption
                        {
                            Text      = qDto.Answers[i].Text,
                            IsCorrect = i == qDto.Correct,
                        });
                    }
                    quiz.Questions.Add(question);
                }
            }

            await _unitOfWork.Quizzes.UpdateAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            var attempts = await _unitOfWork.Quizzes.GetAttemptsByQuizIdAsync(quizId);
            var attList = attempts?.ToList() ?? new List<StudentQuizAttempt>();
            var now = DateTime.UtcNow;

            return Ok(new
            {
                success = true,
                data = new InstructorQuizDto
                {
                    Id          = quiz.Id.ToString(),
                    CourseId    = courseId.ToString(),
                    Title       = quiz.Title,
                    Duration    = (int)(quiz.EndTime - quiz.StartTime).TotalMinutes,
                    Questions   = quiz.Questions?.Count ?? 0,
                    Status      = now < quiz.StartTime ? "upcoming" : now <= quiz.EndTime ? "active" : "ended",
                    StartTime   = quiz.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    EndTime     = quiz.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Submissions = attList.Count,
                    HasAttempts = attList.Count > 0,
                    TotalPoints = (quiz.Questions?.Count ?? 0) * (quiz.GradePerQuestion <= 0 ? 1 : quiz.GradePerQuestion),
                }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  DELETE /api/instructor/courses/{courseId}/quizzes/{quizId}
        //  Hard-deletes quiz, questions, options, attempts, and answers.
        //  Coursework totals re-aggregate on next read.
        // ══════════════════════════════════════════════════════════
        [HttpDelete("courses/{courseId:int}/quizzes/{quizId:int}")]
        public async Task<IActionResult> DeleteQuiz(int courseId, int quizId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var quiz = await _unitOfWork.Quizzes.GetQuizAsync(quizId);
            if (quiz == null || quiz.CourseId != courseId)
                return NotFound(new { success = false, error = new { code = "QUIZ_NOT_FOUND", message = "Quiz not found." } });

            await _unitOfWork.Quizzes.DeleteAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = "Quiz deleted.", data = new { id = quizId } });
        }

        // ══════════════════════════════════════════════════════════
        //  POST /api/instructor/courses/{courseId}/quizzes
        //  Creates quiz + questions + options in one call.
        // ══════════════════════════════════════════════════════════
        [HttpPost("courses/{courseId:int}/quizzes")]
        public async Task<IActionResult> CreateQuiz(
            int courseId,
            [FromBody] CreateInstructorQuizDto dto,
            [FromServices] ICourseworkBudgetService budget)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { success = false, error = new { message = "Title is required." } });

            if (dto.Questions == null || dto.Questions.Count == 0)
                return BadRequest(new { success = false, error = new { message = "At least one question is required." } });

            if (dto.EndTime <= dto.StartTime)
                return BadRequest(new { success = false, error = new { message = "EndTime must be after StartTime." } });

            // Quiz total = (non-empty questions) × points-per-question.
            // GradePerQ defaults to 1 if the client doesn't supply it; it must be 0.5..10.
            int newQuestions = dto.Questions.Count(q => !string.IsNullOrWhiteSpace(q.Text));
            decimal gradePerQ = dto.GradePerQ <= 0m ? 1m : dto.GradePerQ;
            if (gradePerQ < 0.5m || gradePerQ > 10m)
                return BadRequest(new { success = false, error = new { message = "Points per question must be between 0.5 and 10." } });
            decimal newTotalPoints = newQuestions * gradePerQ;
            var bv = await budget.ValidateAddQuizAsync(courseId, newTotalPoints);
            if (!bv.Ok)
                return BadRequest(new { success = false, error = new {
                    code = "COURSEWORK_BUDGET_EXCEEDED",
                    message = bv.Message,
                    used = bv.Used, remaining = bv.Remaining, requested = bv.Requested } });

            // Build quiz with questions/options
            var quiz = new Quiz
            {
                Title            = dto.Title,
                StartTime        = dto.StartTime.ToUniversalTime(),
                EndTime          = dto.EndTime.ToUniversalTime(),
                CourseId         = courseId,
                GradePerQuestion = gradePerQ,
                Questions        = new List<QuizQuestion>(),
            };

            foreach (var qDto in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(qDto.Text)) continue;

                var question = new QuizQuestion
                {
                    QuestionText = qDto.Text,
                    Type         = AYA_UIS.Core.Domain.Enums.QuestionType.MCQ,
                    Options      = new List<QuizOption>(),
                };

                for (int i = 0; i < qDto.Answers.Count; i++)
                {
                    question.Options.Add(new QuizOption
                    {
                        Text      = qDto.Answers[i].Text,
                        IsCorrect = i == qDto.Correct,
                    });
                }

                quiz.Questions.Add(question);
            }

            await _unitOfWork.Quizzes.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            // ── Notify all enrolled students ──
            var quizCourse = await _unitOfWork.Courses.GetByIdAsync(courseId);
            var quizInstructorUser = await _userManager.FindByIdAsync(CurrentUserId!);
            var quizInstructorName = quizInstructorUser?.DisplayName ?? "Instructor";
            var regsForQuiz = await _unitOfWork.Registrations.GetByCourseIdAsync(courseId);
            var quizNotifs = (regsForQuiz ?? Enumerable.Empty<Registration>())
                .Select(r => new Notification
                {
                    UserId         = r.UserId,
                    Type           = "quiz_published",
                    Title          = "New Quiz Available 📝",
                    Body           = $"'{quiz.Title}' is now available in {quizCourse?.Name ?? "your course"}.",
                    CourseId       = courseId,
                    CourseName     = quizCourse?.Name ?? "",
                    QuizId         = quiz.Id,
                    QuizTitle      = quiz.Title,
                    InstructorName = quizInstructorName,
                    IsRead         = false,
                })
                .ToList();
            if (quizNotifs.Count > 0)
                await _notifications.SendManyAsync(quizNotifs);

            return StatusCode(201, new
            {
                success = true,
                data = new InstructorQuizDto
                {
                    Id          = quiz.Id.ToString(),
                    CourseId    = courseId.ToString(),
                    Title       = quiz.Title,
                    Duration    = dto.Duration,
                    Questions   = quiz.Questions.Count,
                    Status      = DateTime.UtcNow < quiz.StartTime ? "upcoming" : "active",
                    StartTime   = quiz.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    EndTime     = quiz.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    TotalPoints = quiz.Questions.Count * quiz.GradePerQuestion,
                }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/quizzes/{quizId}/attempts
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/quizzes/{quizId:int}/attempts")]
        public async Task<IActionResult> GetQuizAttempts(int courseId, int quizId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null || quiz.CourseId != courseId)
                return NotFound();

            var attempts = await _unitOfWork.Quizzes.GetAttemptsByQuizIdAsync(quizId);
            var qCount     = quiz.Questions?.Count ?? 0;
            var quizMaxPts = qCount * (quiz.GradePerQuestion <= 0 ? 1 : quiz.GradePerQuestion);

            var result = new List<QuizSubmissionDto>();
            foreach (var a in attempts ?? Enumerable.Empty<StudentQuizAttempt>())
            {
                var student = await _userManager.FindByIdAsync(a.StudentId);
                result.Add(new QuizSubmissionDto
                {
                    Id          = a.Id.ToString(),
                    StudentId   = a.StudentId,
                    StudentName = student?.DisplayName ?? a.StudentId,
                    SubmittedAt = a.SubmittedAt.ToString("MMM dd · hh:mm tt"),
                    Score       = a.Score,
                    Max         = quizMaxPts,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/coursework-budget
        //  Read-only — used by the assignment/quiz/midterm forms to render
        //  Used X/40 · Remaining Y · Requested Z and pre-block invalid input.
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/coursework-budget")]
        public async Task<IActionResult> GetCourseworkBudget(
            int courseId, [FromServices] ICourseworkBudgetService budget)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var data = await budget.GetBudgetAsync(courseId);
            return Ok(new { success = true, data });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/midterm
        //  Returns all students + their midterm grades for this course.
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/midterm")]
        public async Task<IActionResult> GetMidtermGrades(int courseId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var regs = await _unitOfWork.Registrations.GetByCourseIdAsync(courseId);
            var result = new List<StudentExamGradeDto>();

            foreach (var reg in regs ?? Enumerable.Empty<Registration>())
            {
                var user  = await _userManager.FindByIdAsync(reg.UserId);
                if (user == null) continue;

                var midterm = await _unitOfWork.MidtermGrades.GetAsync(reg.UserId, courseId);
                result.Add(new StudentExamGradeDto
                {
                    StudentId   = reg.UserId,
                    StudentCode = user.Academic_Code ?? "",
                    StudentName = user.DisplayName,
                    Grade       = midterm?.Grade,
                    MaxGrade    = midterm?.Max ?? 20,
                    Submitted   = midterm != null,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  PUT /api/instructor/courses/{courseId}/midterm/{studentId}
        //  Create or update a student's midterm grade.
        // ══════════════════════════════════════════════════════════
        [HttpPut("courses/{courseId:int}/midterm/{studentId}")]
        public async Task<IActionResult> SetMidtermGrade(
            int courseId, string studentId, [FromBody] SetMidtermGradeDto dto,
            [FromServices] ICourseworkBudgetService budget)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            if (dto == null)
                return BadRequest(new { success = false, error = new { message = "Body required." } });

            if (dto.Max <= 0 || dto.Max > 40)
                return BadRequest(new { success = false, error = new { message = "Max must be 1–40." } });

            if (dto.Grade < 0 || dto.Grade > dto.Max)
                return BadRequest(new { success = false, error = new { message = $"Grade must be 0–{dto.Max}." } });

            // Coursework budget — assignments + quizzes + midterm Max <= 40.
            // The midterm Max is treated as a single course-level cap: validation
            // replaces the existing midterm Max rather than adding to it.
            var mv = await budget.ValidateMidtermMaxAsync(courseId, dto.Max);
            if (!mv.Ok)
                return BadRequest(new { success = false, error = new {
                    code = "COURSEWORK_BUDGET_EXCEEDED",
                    message = mv.Message,
                    used = mv.Used, remaining = mv.Remaining, requested = mv.Requested } });

            var existing = await _unitOfWork.MidtermGrades.GetAsync(studentId, courseId);
            if (existing == null)
            {
                await _unitOfWork.MidtermGrades.AddAsync(new MidtermGrade
                {
                    StudentId = studentId,
                    CourseId  = courseId,
                    Grade     = dto.Grade,
                    Max       = dto.Max,
                    Published = dto.Published,
                });
            }
            else
            {
                existing.Grade     = dto.Grade;
                existing.Max       = dto.Max;
                existing.Published = dto.Published;
                await _unitOfWork.MidtermGrades.UpdateAsync(existing);
            }

            // Normalize: keep one consistent course-level midterm Max so the budget
            // service (which uses MAX across rows) cannot drift student-by-student.
            // If any other student's row has a different Max for this course, sync it
            // to dto.Max and clamp the stored Grade so it never exceeds the new cap.
            var courseMidterms = await _unitOfWork.MidtermGrades.GetByCourseAsync(courseId);
            foreach (var mg in courseMidterms)
            {
                if (mg.StudentId == studentId) continue;
                if (mg.Max == dto.Max) continue;
                mg.Max = dto.Max;
                if (mg.Grade > dto.Max) mg.Grade = dto.Max;
                await _unitOfWork.MidtermGrades.UpdateAsync(mg);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, data = new { studentId, courseId, grade = dto.Grade, max = dto.Max, published = dto.Published } });
        }

        // ══════════════════════════════════════════════════════════
        //  PATCH /api/instructor/courses/{courseId}/midterm/publish
        //  Publish or unpublish ALL midterm grades for a course.
        // ══════════════════════════════════════════════════════════
        [HttpPatch("courses/{courseId:int}/midterm/publish")]
        public async Task<IActionResult> PublishMidterm(int courseId, [FromQuery] bool publish = true)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var regs = await _unitOfWork.Registrations.GetByCourseIdAsync(courseId);
            foreach (var reg in regs ?? Enumerable.Empty<Registration>())
            {
                var mg = await _unitOfWork.MidtermGrades.GetAsync(reg.UserId, courseId);
                if (mg == null) continue;
                mg.Published = publish;
                await _unitOfWork.MidtermGrades.UpdateAsync(mg);
            }
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = publish ? "Midterm grades published." : "Midterm grades unpublished." });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/materials
        //  POST /api/instructor/courses/{courseId}/materials
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/materials")]
        public async Task<IActionResult> GetCourseMaterials(int courseId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var course  = await _unitOfWork.Courses.GetByIdAsync(courseId);
            var uploads = await _unitOfWork.CourseUploads.GetByCourseIdAsync(courseId);
            var dtos = uploads
                .OrderByDescending(u => u.UploadedAt)
                .Select(u => new InstructorMaterialDto
                {
                    Id          = u.Id.ToString(),
                    Title       = u.Title,
                    Description = u.Description ?? "",
                    Type        = u.Type.ToString().ToLower(),
                    Url         = u.Url,
                    Date        = u.UploadedAt.ToString("MMM d, yyyy"),
                    Downloads   = 0,
                    ReleaseDate = u.ReleaseDate.HasValue
                                    ? u.ReleaseDate.Value.ToString("yyyy-MM-dd")
                                    : null,
                    Week        = u.Week,
                    CourseCode  = course?.Code ?? "",
                    CourseName  = course?.Name ?? "",
                    FileName    = string.IsNullOrWhiteSpace(u.Url) ? null : Path.GetFileName(u.Url),
                })
                .ToList();

            return Ok(new { success = true, data = dtos });
        }

        // ══════════════════════════════════════════════════════════
        //  PUT /api/instructor/courses/{courseId}/materials/{id}  (multipart)
        //  Replacing the file deletes the old physical file.
        // ══════════════════════════════════════════════════════════
        [HttpPut("courses/{courseId:int}/materials/{materialId:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMaterial(
            int courseId,
            int materialId,
            [FromForm] UpdateMaterialDto dto,
            IFormFile? file,
            [FromForm] string? releaseDate,
            [FromForm] int? week,
            [FromServices] ILocalFileService fileService)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var upload = await _unitOfWork.CourseUploads.GetByIdAsync(materialId);
            if (upload == null || upload.CourseId != courseId)
                return NotFound(new { success = false, error = new { code = "MATERIAL_NOT_FOUND", message = "Material not found." } });

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course is null)
                return NotFound(new { success = false, error = new { code = "COURSE_NOT_FOUND", message = "Course not found." } });

            if (!string.IsNullOrWhiteSpace(dto?.Title))       upload.Title       = dto!.Title!;
            if (dto?.Description != null)                      upload.Description = dto.Description;
            if (!string.IsNullOrWhiteSpace(dto?.Type) &&
                Enum.TryParse<UploadType>(dto!.Type, true, out var t)) upload.Type = t;
            if (week.HasValue)                                 upload.Week        = week.Value;

            if (!string.IsNullOrWhiteSpace(releaseDate) && DateTime.TryParse(releaseDate, out var rdParsed))
                upload.ReleaseDate = rdParsed.ToUniversalTime();

            // ── Replace file: delete old, upload new ──
            if (file is not null)
            {
                if (!string.IsNullOrWhiteSpace(upload.Url))
                    await fileService.DeleteFileByUrlAsync(upload.Url);

                var fileId  = Guid.NewGuid().ToString();
                var newUrl  = await fileService.UploadCourseFileAsync(file, fileId, course.Name, upload.Type, CancellationToken.None);
                upload.FileId = fileId;
                upload.Url    = newUrl ?? string.Empty;
            }

            await _unitOfWork.CourseUploads.Update(upload);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new InstructorMaterialDto
                {
                    Id          = upload.Id.ToString(),
                    Title       = upload.Title,
                    Description = upload.Description ?? "",
                    Type        = upload.Type.ToString().ToLower(),
                    Url         = upload.Url,
                    Date        = upload.UploadedAt.ToString("MMM d, yyyy"),
                    Downloads   = 0,
                    ReleaseDate = upload.ReleaseDate.HasValue
                                    ? upload.ReleaseDate.Value.ToString("yyyy-MM-dd")
                                    : null,
                    Week        = upload.Week,
                    CourseCode  = course.Code,
                    CourseName  = course.Name,
                    FileName    = string.IsNullOrWhiteSpace(upload.Url) ? null : Path.GetFileName(upload.Url),
                }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  DELETE /api/instructor/courses/{courseId}/materials/{id}
        //  Hard-deletes the upload and removes the physical file.
        // ══════════════════════════════════════════════════════════
        [HttpDelete("courses/{courseId:int}/materials/{materialId:int}")]
        public async Task<IActionResult> DeleteMaterial(
            int courseId,
            int materialId,
            [FromServices] ILocalFileService fileService)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var upload = await _unitOfWork.CourseUploads.GetByIdAsync(materialId);
            if (upload == null || upload.CourseId != courseId)
                return NotFound(new { success = false, error = new { code = "MATERIAL_NOT_FOUND", message = "Material not found." } });

            if (!string.IsNullOrWhiteSpace(upload.Url))
                await fileService.DeleteFileByUrlAsync(upload.Url);

            await _unitOfWork.CourseUploads.Delete(upload);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = "Material deleted.", data = new { id = materialId } });
        }

        [HttpPost("courses/{courseId:int}/materials")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMaterial(
            int courseId,
            [FromForm] CreateMaterialDto dto,
            IFormFile? file,
            [FromForm] string? releaseDate,
            [FromForm] int? week,
            [FromServices] ILocalFileService fileService)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course is null)
                return NotFound(new { success = false, error = new { code = "COURSE_NOT_FOUND", message = "Course not found." } });

            string? fileUrl = null;
            var fileId = Guid.NewGuid().ToString();

            if (file is not null)
                fileUrl = await fileService.UploadCourseFileAsync(file, fileId, course.Name, UploadType.Lecture, CancellationToken.None);

            DateTime? rd = null;
            if (!string.IsNullOrWhiteSpace(releaseDate) && DateTime.TryParse(releaseDate, out var rdParsed))
                rd = rdParsed.ToUniversalTime();

            var upload = new CourseUpload
            {
                Title             = dto.Title,
                Description       = dto.Description ?? "",
                Type              = UploadType.Lecture,
                FileId            = fileUrl is not null ? fileId : "",
                Url               = fileUrl ?? "",
                UploadedByUserId  = CurrentUserId!,
                CourseId          = courseId,
                UploadedAt        = DateTime.UtcNow,
                ReleaseDate       = rd,
                Week              = week,
            };

            await _unitOfWork.CourseUploads.AddAsync(upload);
            await _unitOfWork.SaveChangesAsync();

            // ── Notify all enrolled students ──
            var lectureInstructorUser = await _userManager.FindByIdAsync(CurrentUserId!);
            var lectureInstructorName = lectureInstructorUser?.DisplayName ?? "Instructor";
            var regsForLec = await _unitOfWork.Registrations.GetByCourseIdAsync(courseId);
            var lecNotifs = (regsForLec ?? Enumerable.Empty<Registration>())
                .Select(r => new Notification
                {
                    UserId         = r.UserId,
                    Type           = "lecture_uploaded",
                    Title          = "New Lecture Uploaded 🎬",
                    Body           = $"'{dto.Title}' has been uploaded to {course.Name}.",
                    CourseId       = courseId,
                    CourseName     = course.Name,
                    LectureId      = upload.Id,
                    LectureTitle   = dto.Title,
                    InstructorName = lectureInstructorName,
                    IsRead         = false,
                })
                .ToList();
            if (lecNotifs.Count > 0)
                await _notifications.SendManyAsync(lecNotifs);

            return StatusCode(201, new
            {
                success = true,
                data = new InstructorMaterialDto
                {
                    Id          = upload.Id.ToString(),
                    Title       = upload.Title,
                    Type        = "lecture",
                    Url         = upload.Url,
                    Date        = upload.UploadedAt.ToString("MMM d, yyyy"),
                    Downloads   = 0,
                    ReleaseDate = upload.ReleaseDate.HasValue
                                    ? upload.ReleaseDate.Value.ToString("yyyy-MM-dd")
                                    : null,
                }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/final-grade/status
        //  Returns whether the 24-hour post-exam lock has lifted.
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/final-grade/status")]
        public async Task<IActionResult> GetFinalGradeStatus(int courseId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            // Find the final exam schedule entry for this course
            var allExams = await _unitOfWork.ExamSchedules.GetAllAsync();
            var finalExam = allExams.FirstOrDefault(e =>
                e.CourseId == courseId &&
                string.Equals(e.Type, "final", StringComparison.OrdinalIgnoreCase));

            if (finalExam == null)
            {
                // No final exam scheduled — allow grading (no lock)
                return Ok(new
                {
                    success = true,
                    data = new FinalGradeStatusDto { Locked = false }
                });
            }

            var examEnd    = finalExam.Date.Date + TimeSpan.FromHours(finalExam.StartTime + finalExam.Duration);
            var unlockAt   = examEnd.AddHours(24);
            var locked     = DateTime.UtcNow < unlockAt;

            var startHour  = TimeSpan.FromHours(finalExam.StartTime);
            var endHour    = TimeSpan.FromHours(finalExam.StartTime + finalExam.Duration);

            return Ok(new
            {
                success = true,
                data = new FinalGradeStatusDto
                {
                    Locked    = locked,
                    ExamDate  = finalExam.Date.ToString("yyyy-MM-dd"),
                    ExamTime  = $"{startHour:hh\\:mm} – {endHour:hh\\:mm}",
                    UnlockAt  = unlockAt.ToString("o"),
                }
            });
        }

        // ══════════════════════════════════════════════════════════
        //  GET /api/instructor/courses/{courseId}/final-grade/students
        //  Returns each enrolled student with coursework breakdown.
        // ══════════════════════════════════════════════════════════
        [HttpGet("courses/{courseId:int}/final-grade/students")]
        public async Task<IActionResult> GetFinalGradeStudents(int courseId)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            var regs = (await _unitOfWork.Registrations.GetByCourseIdAsync(courseId))
                       ?? Enumerable.Empty<Registration>();

            // Pre-load quizzes and assignments for this course once.
            // Note: GetQuizzesByCourseId / GetAssignmentsByCourseIdAsync already filter
            // archived rows from Reset Material, so they never count toward grading here.
            var quizzes     = (await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseId)).ToList();
            var assignments = (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseId)).ToList();
            var now = DateTime.UtcNow;

            var result = new List<FinalGradeStudentDto>();

            foreach (var reg in regs)
            {
                // Skip dropped/rejected registrations; allow both Pending (student self-registered)
                // and Approved (admin force-added) — both are "active" in Student Courses.
                if (reg.Status != RegistrationStatus.Approved && reg.Status != RegistrationStatus.Pending)
                    continue;

                var user = await _userManager.FindByIdAsync(reg.UserId);
                if (user == null) continue;

                // ── Midterm ──
                var midterm    = await _unitOfWork.MidtermGrades.GetAsync(reg.UserId, courseId);
                var midGrade   = midterm?.Grade ?? 0;
                var midMax     = midterm?.Max ?? 0;

                // ── Quizzes (deadline-aware) ──
                // Before quiz EndTime: missing attempt = pending, NOT explicit 0.
                // After quiz EndTime:  missing attempt = explicit 0 (no contribution).
                decimal quizScore = 0;
                int pendingQuizzes = 0;
                int missedQuizzes  = 0;
                foreach (var q in quizzes)
                {
                    var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, reg.UserId);
                    if (attempt != null)
                    {
                        quizScore += attempt.Score;
                    }
                    else if (now > q.EndTime)
                    {
                        // Quiz window closed without an attempt → explicit 0 contribution.
                        missedQuizzes++;
                    }
                    else
                    {
                        // Quiz still open / not yet started → pending, not zero yet.
                        pendingQuizzes++;
                    }
                }

                // ── Assignments (deadline-aware) ──
                // Before assignment Deadline: missing submission = pending, NOT explicit 0.
                // After assignment Deadline:  missing submission = explicit 0 (no contribution).
                decimal asnScore = 0;
                int pendingAssignments = 0;
                int missedAssignments  = 0;
                foreach (var a in assignments)
                {
                    var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, reg.UserId);
                    if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                    {
                        asnScore += sub.Grade ?? 0;
                    }
                    else if (now > a.Deadline)
                    {
                        // Past deadline with no Accepted submission → explicit 0 contribution.
                        missedAssignments++;
                    }
                    else
                    {
                        // Before deadline → pending, not zero yet.
                        pendingAssignments++;
                    }
                }

                // ── Stored final grade ──
                var stored = await _unitOfWork.FinalGrades.GetAsync(reg.UserId, courseId);
                var bonus  = stored?.Bonus ?? 0;

                decimal cwTotal = Math.Min(40m, midGrade + quizScore + asnScore + bonus);
                decimal? total  = stored != null ? cwTotal + stored.FinalScore : null;

                string? letter = total.HasValue ? ToLetter((int)Math.Round(total.Value)) : null;

                result.Add(new FinalGradeStudentDto
                {
                    StudentId          = reg.UserId,
                    StudentCode        = user.Academic_Code ?? "",
                    StudentName        = user.DisplayName,
                    MidtermGrade       = midGrade,
                    MidtermMax         = midMax,
                    QuizScore          = Math.Round(quizScore, 1),
                    AssignmentScore    = Math.Round(asnScore, 1),
                    Bonus              = bonus,
                    CourseworkTotal    = Math.Round(cwTotal, 1),
                    FinalScore         = stored?.FinalScore,
                    Total              = total.HasValue ? Math.Round(total.Value, 1) : null,
                    LetterGrade        = letter,
                    Submitted          = stored != null,
                    PendingAssignments = pendingAssignments,
                    PendingQuizzes     = pendingQuizzes,
                    MissedAssignments  = missedAssignments,
                    MissedQuizzes      = missedQuizzes,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  PUT /api/instructor/courses/{courseId}/final-grade/{studentId}
        //  Save or update a student's final exam grade.
        // ══════════════════════════════════════════════════════════
        [HttpPut("courses/{courseId:int}/final-grade/{studentId}")]
        public async Task<IActionResult> SetFinalGrade(
            int courseId, string studentId, [FromBody] SetFinalGradeDto dto)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            if (dto == null)
                return BadRequest(new { success = false, error = new { message = "Body required." } });

            if (dto.FinalScore < 0 || dto.FinalScore > 60)
                return BadRequest(new { success = false, error = new { message = "FinalScore must be 0–60." } });

            // Bonus is upper-bounded by the coursework cap; final per-student
            // ceiling is computed below from earned coursework. Reject only
            // values that are nonsensical (negative or > 40).
            if (dto.Bonus < 0 || dto.Bonus > 40)
                return BadRequest(new { success = false, error = new { message = "Bonus must be 0–40 and is further limited by the student's earned coursework." } });

            // Block save if the student has no midterm grade recorded
            var midterm = await _unitOfWork.MidtermGrades.GetAsync(studentId, courseId);
            if (midterm == null)
                return BadRequest(new
                {
                    success = false,
                    error = new { message = "Midterm grade must be recorded before saving the final grade." }
                });

            // Check registration still active (not dropped)
            var reg = (await _unitOfWork.Registrations.GetByCourseIdAsync(courseId))
                      ?.FirstOrDefault(r => r.UserId == studentId);
            if (reg == null)
                return NotFound(new { success = false, error = new { message = "Student is not enrolled in this course." } });
            if (reg.Status != RegistrationStatus.Approved && reg.Status != RegistrationStatus.Pending)
                return BadRequest(new { success = false, error = new { message = "Student is not actively enrolled in this course." } });

            // ── Block if any coursework deadline has not yet passed (items still pending) ──
            {
                var pendingQuizList = (await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseId)).ToList();
                var pendingAsnList  = (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseId)).ToList();
                var nowCheck        = DateTime.UtcNow;
                int pendingQ = 0, pendingA = 0;
                foreach (var q in pendingQuizList)
                {
                    if (nowCheck <= q.EndTime)
                    {
                        var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, studentId);
                        if (attempt == null) pendingQ++;
                    }
                }
                foreach (var a in pendingAsnList)
                {
                    if (nowCheck <= a.Deadline)
                    {
                        var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, studentId);
                        if (sub == null || !string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                            pendingA++;
                    }
                }
                if (pendingA > 0 || pendingQ > 0)
                    return UnprocessableEntity(new
                    {
                        success = false,
                        error = new
                        {
                            code    = "PENDING_COURSEWORK",
                            message = $"Cannot finalize this student's grade yet. There are still pending coursework items before their deadline. Pending assignments: {pendingA} Pending quizzes: {pendingQ}"
                        }
                    });
            }

            // ── Bonus only fills the gap up to 40. Recompute here so the rule
            // cannot be bypassed by a hand-crafted payload.
            // Deadline rule: missing submission/attempt counts as 0 either before or
            // after the deadline (it just doesn't add anything). The "before deadline =
            // pending" distinction is reported to the UI via PendingAssignments /
            // PendingQuizzes; numerically the contribution is 0 in both cases. ──
            decimal cwQuizForCap = 0, cwAsnForCap = 0;
            var quizzesForCap = (await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseId)).ToList();
            foreach (var q in quizzesForCap)
            {
                var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, studentId);
                if (attempt != null) cwQuizForCap += attempt.Score;
            }
            var asnsForCap = (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseId)).ToList();
            foreach (var a in asnsForCap)
            {
                var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, studentId);
                if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                    cwAsnForCap += sub.Grade ?? 0;
            }
            decimal coreCw  = midterm.Grade + cwQuizForCap + cwAsnForCap;
            // Bonus may fill the gap between earned coursework and 40 — no extra
            // 10-point ceiling. allowedBonus = max(0, 40 - courseworkBeforeBonus).
            int maxBonus    = (int)Math.Max(0m, 40m - coreCw);
            int effectiveBonus = Math.Min(dto.Bonus, maxBonus);
            if (effectiveBonus < 0) effectiveBonus = 0;
            dto.Bonus = effectiveBonus;

            var existing = await _unitOfWork.FinalGrades.GetAsync(studentId, courseId);
            if (existing == null)
            {
                await _unitOfWork.FinalGrades.AddAsync(new FinalGrade
                {
                    StudentId       = studentId,
                    CourseId        = courseId,
                    FinalScore      = dto.FinalScore,
                    Bonus           = dto.Bonus,
                    AdminFinalTotal = null, // instructor grade takes precedence
                    Published       = false,
                });
            }
            else
            {
                existing.FinalScore      = dto.FinalScore;
                existing.Bonus           = dto.Bonus;
                existing.AdminFinalTotal = null; // clear admin override; instructor grade is now authoritative
                await _unitOfWork.FinalGrades.UpdateAsync(existing);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another instructor's write landed between our read and write.
                return Conflict(new
                {
                    success = false,
                    error   = new { code = "CONCURRENT_WRITE", message = "Another instructor updated this student's grade at the same time. Please refresh and try again." }
                });
            }
            catch (DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("UNIQUE",  StringComparison.OrdinalIgnoreCase) == true
                   || ex.InnerException?.Message.Contains("unique",  StringComparison.OrdinalIgnoreCase) == true
                   || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Two concurrent first-time saves for the same student+course.
                return Conflict(new
                {
                    success = false,
                    error   = new { code = "CONCURRENT_INSERT", message = "Another instructor saved a grade for this student at the same time. Please refresh and try again." }
                });
            }

            // Recompute coursework for response
            var quizzes     = (await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseId)).ToList();
            var assignments = (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseId)).ToList();
            decimal quizScore = 0, asnScore = 0;
            foreach (var q in quizzes)
            {
                var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, studentId);
                if (attempt != null) quizScore += attempt.Score;
            }
            foreach (var a in assignments)
            {
                var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, studentId);
                if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                    asnScore += sub.Grade ?? 0;
            }

            decimal cwTotal = Math.Min(40m, midterm.Grade + quizScore + asnScore + dto.Bonus);
            decimal total   = cwTotal + dto.FinalScore;

            return Ok(new
            {
                success = true,
                data = new
                {
                    studentId,
                    courseId,
                    finalScore      = dto.FinalScore,
                    bonus           = dto.Bonus,
                    courseworkTotal = Math.Round(cwTotal, 1),
                    total           = Math.Round(total, 1),
                    letterGrade     = ToLetter((int)Math.Round(total)),
                }
            });
        }

        /// <summary>Maps a 0–100 total to a letter grade.</summary>
        private static string ToLetter(int total) => total switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _     => "F",
        };
    }
}
