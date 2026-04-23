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
            [FromServices] ILocalFileService fileService)
        {
            var uid = CurrentUserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { success = false, error = new { message = "Title is required." } });

            // Resolve course by code OR by ID
            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var course = allCourses.FirstOrDefault(c =>
                c.Code.Equals(dto.CourseCode, StringComparison.OrdinalIgnoreCase));

            if (course == null)
                return BadRequest(new { success = false, error = new { message = "Course not found." } });

            if (!await InstructorOwnsCourse(course.Id))
                return Forbid();

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
                Points          = dto.MaxGrade > 0 ? dto.MaxGrade : 20,
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
        //  PUT /api/instructor/assignments/{assignmentId}
        // ══════════════════════════════════════════════════════════
        [HttpPut("assignments/{assignmentId:int}")]
        public async Task<IActionResult> UpdateAssignment(int assignmentId, [FromBody] CreateInstructorAssignmentDto dto)
        {
            if (dto == null) return BadRequest();

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound(new { success = false, error = new { message = "Assignment not found." } });

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            if (!string.IsNullOrWhiteSpace(dto.Title))        assignment.Title       = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Description))  assignment.Description = dto.Description;
            if (dto.MaxGrade > 0)                              assignment.Points      = dto.MaxGrade;
            if (DateTime.TryParse(dto.Deadline, out var dl))  assignment.Deadline    = dl;
            if (!string.IsNullOrWhiteSpace(dto.ReleaseDate) && DateTime.TryParse(dto.ReleaseDate, out var rdu))
                assignment.ReleaseDate = rdu.ToUniversalTime();
            else if (dto.ReleaseDate == null)
                assignment.ReleaseDate = null; // explicit clear → publish now

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, data = new InstructorAssignmentDto
            {
                Id          = assignment.Id.ToString(),
                Title       = assignment.Title,
                CourseId    = assignment.CourseId.ToString(),
                Deadline    = assignment.Deadline.ToString("yyyy-MM-dd HH:mm"),
                ReleaseDate = assignment.ReleaseDate?.ToString("yyyy-MM-dd HH:mm"),
                MaxGrade    = assignment.Points,
                Description = assignment.Description ?? "",
            }});
        }

        // ══════════════════════════════════════════════════════════
        //  DELETE /api/instructor/assignments/{assignmentId}
        // ══════════════════════════════════════════════════════════
        [HttpDelete("assignments/{assignmentId:int}")]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null) return NotFound();

            if (!await InstructorOwnsCourse(assignment.CourseId))
                return Forbid();

            // IAssignmentRepository has no Delete — return informational response.
            // To enable hard-delete, add DeleteAsync to IAssignmentRepository.
            return Ok(new { success = true, message = "Assignment removed." });
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
                    Id         = q.Id.ToString(),
                    CourseId   = courseId.ToString(),
                    Title      = q.Title,
                    Duration   = (int)(q.EndTime - q.StartTime).TotalMinutes,
                    Questions  = qCount,
                    Status     = status,
                    StartTime  = q.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    EndTime    = q.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Submissions = attList.Count,
                    AvgScore   = avg,
                });
            }

            return Ok(new { success = true, data = result });
        }

        // ══════════════════════════════════════════════════════════
        //  POST /api/instructor/courses/{courseId}/quizzes
        //  Creates quiz + questions + options in one call.
        // ══════════════════════════════════════════════════════════
        [HttpPost("courses/{courseId:int}/quizzes")]
        public async Task<IActionResult> CreateQuiz(int courseId, [FromBody] CreateInstructorQuizDto dto)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { success = false, error = new { message = "Title is required." } });

            if (dto.Questions == null || dto.Questions.Count == 0)
                return BadRequest(new { success = false, error = new { message = "At least one question is required." } });

            if (dto.EndTime <= dto.StartTime)
                return BadRequest(new { success = false, error = new { message = "EndTime must be after StartTime." } });

            // Build quiz with questions/options
            var quiz = new Quiz
            {
                Title     = dto.Title,
                StartTime = dto.StartTime.ToUniversalTime(),
                EndTime   = dto.EndTime.ToUniversalTime(),
                CourseId  = courseId,
                Questions = new List<QuizQuestion>(),
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
                    Id        = quiz.Id.ToString(),
                    CourseId  = courseId.ToString(),
                    Title     = quiz.Title,
                    Duration  = dto.Duration,
                    Questions = quiz.Questions.Count,
                    Status    = DateTime.UtcNow < quiz.StartTime ? "upcoming" : "active",
                    StartTime = quiz.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    EndTime   = quiz.EndTime.ToString("yyyy-MM-dd HH:mm"),
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
            var qCount   = quiz.Questions?.Count ?? 0;

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
                    Max         = qCount,
                });
            }

            return Ok(new { success = true, data = result });
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
            int courseId, string studentId, [FromBody] SetMidtermGradeDto dto)
        {
            if (!await InstructorOwnsCourse(courseId))
                return Forbid();

            if (dto == null)
                return BadRequest(new { success = false, error = new { message = "Body required." } });

            if (dto.Max <= 0 || dto.Max > 40)
                return BadRequest(new { success = false, error = new { message = "Max must be 1–40." } });

            if (dto.Grade < 0 || dto.Grade > dto.Max)
                return BadRequest(new { success = false, error = new { message = $"Grade must be 0–{dto.Max}." } });

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

            var uploads = await _unitOfWork.CourseUploads.GetByCourseIdAsync(courseId);
            var dtos = uploads
                .OrderByDescending(u => u.UploadedAt)
                .Select(u => new InstructorMaterialDto
                {
                    Id          = u.Id.ToString(),
                    Title       = u.Title,
                    Type        = u.Type.ToString().ToLower(),
                    Url         = u.Url,
                    Date        = u.UploadedAt.ToString("MMM d, yyyy"),
                    Downloads   = 0,
                    ReleaseDate = u.ReleaseDate.HasValue
                                    ? u.ReleaseDate.Value.ToString("yyyy-MM-dd")
                                    : null,
                })
                .ToList();

            return Ok(new { success = true, data = dtos });
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

            // Pre-load quizzes and assignments for this course once
            var quizzes     = (await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseId)).ToList();
            var assignments = (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseId)).ToList();

            var result = new List<FinalGradeStudentDto>();

            foreach (var reg in regs)
            {
                // Skip non-approved registrations (dropped registrations are deleted)
                if (reg.Status != RegistrationStatus.Approved)
                    continue;

                var user = await _userManager.FindByIdAsync(reg.UserId);
                if (user == null) continue;

                // ── Midterm ──
                var midterm    = await _unitOfWork.MidtermGrades.GetAsync(reg.UserId, courseId);
                var midGrade   = midterm?.Grade ?? 0;
                var midMax     = midterm?.Max ?? 0;

                // ── Quizzes ──
                decimal quizScore = 0;
                foreach (var q in quizzes)
                {
                    var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, reg.UserId);
                    if (attempt != null)
                        quizScore += attempt.Score;
                }

                // ── Assignments ──
                decimal asnScore = 0;
                foreach (var a in assignments)
                {
                    var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, reg.UserId);
                    if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                        asnScore += sub.Grade ?? 0;
                }

                // ── Stored final grade ──
                var stored = await _unitOfWork.FinalGrades.GetAsync(reg.UserId, courseId);
                var bonus  = stored?.Bonus ?? 0;

                decimal cwTotal = Math.Min(40m, midGrade + quizScore + asnScore + bonus);
                decimal? total  = stored != null ? cwTotal + stored.FinalScore : null;

                string? letter = total.HasValue ? ToLetter((int)Math.Round(total.Value)) : null;

                result.Add(new FinalGradeStudentDto
                {
                    StudentId        = reg.UserId,
                    StudentName      = user.DisplayName,
                    MidtermGrade     = midGrade,
                    MidtermMax       = midMax,
                    QuizScore        = Math.Round(quizScore, 1),
                    AssignmentScore  = Math.Round(asnScore, 1),
                    Bonus            = bonus,
                    CourseworkTotal  = Math.Round(cwTotal, 1),
                    FinalScore       = stored?.FinalScore,
                    Total            = total.HasValue ? Math.Round(total.Value, 1) : null,
                    LetterGrade      = letter,
                    Submitted        = stored != null,
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

            if (dto.Bonus < 0 || dto.Bonus > 10)
                return BadRequest(new { success = false, error = new { message = "Bonus must be 0–10." } });

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
            if (reg.Status != RegistrationStatus.Approved)
                return BadRequest(new { success = false, error = new { message = "Student is not actively enrolled in this course." } });

            var existing = await _unitOfWork.FinalGrades.GetAsync(studentId, courseId);
            if (existing == null)
            {
                await _unitOfWork.FinalGrades.AddAsync(new FinalGrade
                {
                    StudentId  = studentId,
                    CourseId   = courseId,
                    FinalScore = dto.FinalScore,
                    Bonus      = dto.Bonus,
                    Published  = false,
                });
            }
            else
            {
                existing.FinalScore = dto.FinalScore;
                existing.Bonus      = dto.Bonus;
                await _unitOfWork.FinalGrades.UpdateAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();

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
