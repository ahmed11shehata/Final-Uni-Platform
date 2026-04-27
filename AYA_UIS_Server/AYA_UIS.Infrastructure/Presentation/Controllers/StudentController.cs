using System.Security.Claims;
using Abstraction.Contracts;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Student_Module;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRegistrationService _registrationService;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminService _adminService;
        private readonly ILocalFileService _fileService;
        private readonly INotificationService _notifications;

        public StudentController(
            IStudentRegistrationService registrationService,
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            IAdminService adminService,
            ILocalFileService fileService,
            INotificationService notifications)
        {
            _registrationService = registrationService;
            _userManager         = userManager;
            _unitOfWork          = unitOfWork;
            _adminService        = adminService;
            _fileService         = fileService;
            _notifications       = notifications;
        }

        /// <summary>
        /// GET /api/student/profile
        /// Returns the logged-in student's own profile data
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<StudentProfileDto>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "User not found." } });

            var roles = await _userManager.GetRolesAsync(user);

            string? departmentName = null;
            if (user.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value);
                departmentName = dept?.Name;
            }

            return Ok(new { success = true, data = MapProfile(user, roles, departmentName) });
        }

        /// <summary>
        /// PUT /api/student/profile
        /// Update student profile (name, phone, address, avatar, dob, gender)
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<StudentProfileDto>> UpdateProfile([FromBody] UpdateStudentProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "User not found." } });

            // Name is read-only after account creation — skip it
            if (dto.Phone != null) user.PhoneNumber = dto.Phone;
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.Avatar != null) user.ProfilePicture = dto.Avatar;
            if (dto.Gender != null && Enum.TryParse<Gender>(dto.Gender, true, out var genderEnum))
                user.Gender = genderEnum;
            if (dto.Dob != null && DateTime.TryParse(dto.Dob, out var dob))
                user.DateOfBirth = dob;

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            string? departmentName = null;
            if (user.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value);
                departmentName = dept?.Name;
            }

            return Ok(new { success = true, data = MapProfile(user, roles, departmentName) });
        }

        private static StudentProfileDto MapProfile(User user, IList<string> roles, string? departmentName) =>
            new StudentProfileDto
            {
                Id = user.Academic_Code ?? user.Id,
                Name = user.DisplayName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault()?.ToLower() ?? "student",
                Department = departmentName ?? "",
                Year = user.Level?.ToString()?.Replace("_", " ") ?? "",
                Gender = user.Gender.ToString().ToLower(),
                Phone = user.PhoneNumber ?? "",
                Address = user.Address ?? "",
                Avatar = string.IsNullOrEmpty(user.ProfilePicture) ? null : user.ProfilePicture,
                EntryYear = int.TryParse(user.EntryYear, out var ey) ? ey : 0,
                Dob = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? ""
            };

        /// <summary>
        /// GET /api/student/courses
        /// Returns list of courses the student is registered for
        /// </summary>
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.GetEnrolledCoursesAsync(userId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// GET /api/student/courses/{courseId}
        /// Returns full details of a specific enrolled course
        /// </summary>
        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetCourseDetail(string courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.GetCourseDetailAsync(userId, courseId);
            return Ok(new { success = true, data = result });
        }

        private const int MaxSubmissionAttempts = 4;

        /// <summary>
        /// POST /api/student/assignments/{assignmentId}/submit
        /// First submission or resubmission after a confirmed soft-clear.
        /// Requires deletion of an existing active submission before resubmitting.
        /// Enforces a maximum of 4 total attempts.
        /// </summary>
        [HttpPost("assignments/{assignmentId}/submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitAssignment(
            int assignmentId,
            IFormFile file)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, error = new { code = "NO_FILE", message = "File is required." } });

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Assignment not found." } });

            if (DateTime.UtcNow > assignment.Deadline)
                return BadRequest(new { success = false, error = new { code = "DEADLINE_PASSED", message = "Submission deadline has passed." } });

            var enrolled = await _unitOfWork.Registrations.IsUserRegisteredInCourseAsync(userId, assignment.CourseId);
            if (!enrolled)
                return StatusCode(403, new { success = false, error = new { code = "NOT_ENROLLED", message = "You are not enrolled in this course." } });

            var existing = await _unitOfWork.Assignments.GetStudentSubmissionAsync(assignmentId, userId);

            if (existing != null)
            {
                // Active submission — student must delete it first before resubmitting
                if (existing.Status == "Pending")
                    return BadRequest(new { success = false, error = new { code = "MUST_DELETE_FIRST", message = "You must delete your existing submission before uploading a new one." } });

                if (existing.Status == "Accepted")
                    return BadRequest(new { success = false, error = new { code = "ALREADY_GRADED", message = "This submission has already been graded." } });

                if (existing.Status == "Rejected")
                    return BadRequest(new { success = false, error = new { code = "REJECTED", message = "Rejected submissions cannot be resubmitted." } });

                // Status == "Cleared" — student deleted their previous submission; check attempt limit
                if (existing.AttemptCount >= MaxSubmissionAttempts)
                    return BadRequest(new { success = false, error = new { code = "MAX_ATTEMPTS", message = $"Maximum of {MaxSubmissionAttempts} submission attempts reached." } });

                var fileId  = Guid.NewGuid().ToString();
                var fileUrl = await _fileService.UploadSubmissionFileAsync(file, fileId, assignmentId, HttpContext.RequestAborted);

                existing.FileUrl      = fileUrl;
                existing.SubmittedAt  = DateTime.UtcNow;
                existing.Status       = "Pending";
                existing.Grade        = null;
                existing.AttemptCount += 1;
                await _unitOfWork.Assignments.UpdateSubmissionAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                // Notify course instructor(s) — resubmission
                await NotifyInstructorsOfSubmission(userId, assignment, isResubmit: true);
            }
            else
            {
                // First-ever submission
                var fileId  = Guid.NewGuid().ToString();
                var fileUrl = await _fileService.UploadSubmissionFileAsync(file, fileId, assignmentId, HttpContext.RequestAborted);

                var submission = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    StudentId    = userId,
                    FileUrl      = fileUrl,
                    SubmittedAt  = DateTime.UtcNow,
                    Status       = "Pending",
                    AttemptCount = 1,
                };
                await _unitOfWork.Assignments.AddSubmissionAsync(submission);
                await _unitOfWork.SaveChangesAsync();

                // Notify course instructor(s) — first submission
                await NotifyInstructorsOfSubmission(userId, assignment, isResubmit: false);
            }

            return Ok(new { success = true, data = new { message = "Assignment submitted successfully." } });
        }

        /// <summary>
        /// DELETE /api/student/assignments/{assignmentId}/submit
        /// Soft-clears the submission (preserves AttemptCount for limit enforcement).
        /// Blocked after deadline or when max attempts already reached.
        /// </summary>
        [HttpDelete("assignments/{assignmentId}/submit")]
        public async Task<IActionResult> RemoveSubmission(int assignmentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Assignment not found." } });

            if (DateTime.UtcNow > assignment.Deadline)
                return BadRequest(new { success = false, error = new { code = "DEADLINE_PASSED", message = "Cannot remove submission after deadline." } });

            var existing = await _unitOfWork.Assignments.GetStudentSubmissionAsync(assignmentId, userId);
            if (existing == null || existing.Status == "Cleared")
                return NotFound(new { success = false, error = new { code = "NO_SUBMISSION", message = "No active submission found." } });

            if (existing.Status == "Rejected" || existing.Status == "Accepted")
                return BadRequest(new { success = false, error = new { code = "GRADED", message = "Cannot remove a graded or rejected submission." } });

            if (existing.AttemptCount >= MaxSubmissionAttempts)
                return BadRequest(new { success = false, error = new { code = "MAX_ATTEMPTS", message = $"Maximum of {MaxSubmissionAttempts} submission attempts reached. Deletion would prevent a new upload." } });

            // Soft-clear: keep record (and AttemptCount) so the limit is preserved
            existing.FileUrl = string.Empty;
            existing.Status  = "Cleared";
            await _unitOfWork.Assignments.UpdateSubmissionAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, data = new { message = "Submission removed. You may upload a new file.", attemptsUsed = existing.AttemptCount, attemptsRemaining = MaxSubmissionAttempts - existing.AttemptCount } });
        }

        /// <summary>
        /// GET /api/student/notifications
        /// Returns the student's notifications.
        /// </summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var notifs = await _unitOfWork.Notifications.GetForUserAsync(userId);
            var result = notifs.Select(n => new StudentNotificationDto
            {
                Id     = n.Id,
                Type   = n.Type,
                Title  = n.Title,
                Body   = n.Body,
                IsRead = n.IsRead,
                Time   = FormatTimeAgo(n.CreatedAt),
                Detail = new StudentNotificationDetailDto
                {
                    CourseName      = n.CourseName,
                    CourseId        = n.CourseId,
                    AssignmentTitle = n.AssignmentTitle,
                    AssignmentId    = n.AssignmentId,
                    Grade           = n.Grade,
                    Max             = n.Max,
                    RejectionReason = n.RejectionReason,
                    QuizTitle       = n.QuizTitle,
                    QuizId          = n.QuizId,
                    LectureTitle    = n.LectureTitle,
                    LectureId       = n.LectureId,
                    InstructorName  = n.InstructorName,
                    StudentName     = n.StudentName,
                    StudentCode     = n.StudentCode,
                    TargetStudentId = n.TargetStudentId,
                }
            }).ToList();

            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// PUT /api/student/notifications/{id}/read
        /// Mark a notification as read.
        /// </summary>
        [HttpPut("notifications/{id:int}/read")]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            await _unitOfWork.Notifications.MarkReadAsync(id, userId);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true });
        }

        /// <summary>
        /// PUT /api/student/notifications/read-all
        /// Mark all notifications as read.
        /// </summary>
        [HttpPut("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            await _unitOfWork.Notifications.MarkAllReadAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { success = true });
        }

        /// <summary>
        /// Sends a notification to every instructor assigned to the course
        /// when a student submits or resubmits an assignment.
        /// </summary>
        private async Task NotifyInstructorsOfSubmission(
            string studentId, Assignment assignment, bool isResubmit)
        {
            try
            {
                var student = await _userManager.FindByIdAsync(studentId);
                var course  = await _unitOfWork.Courses.GetByIdAsync(assignment.CourseId);
                var instructors = await _unitOfWork.RegistrationCourseInstructors
                    .GetByCourseAsync(assignment.CourseId);

                var notifs = instructors.Select(ins => new Notification
                {
                    UserId          = ins.InstructorId,
                    Type            = isResubmit ? "submission_updated" : "submission_new",
                    Title           = isResubmit ? "Assignment Resubmitted 🔄" : "New Submission 📬",
                    Body            = $"{student?.DisplayName ?? "A student"} submitted '{assignment.Title}'" +
                                      $" — {course?.Name ?? "course"}.",
                    CourseId        = assignment.CourseId,
                    CourseName      = course?.Name ?? "",
                    AssignmentId    = assignment.Id,
                    AssignmentTitle = assignment.Title,
                    StudentName     = student?.DisplayName ?? "",
                    StudentCode     = student?.Academic_Code ?? "",
                    TargetStudentId = studentId,
                    IsRead          = false,
                }).ToList();

                if (notifs.Count > 0)
                    await _notifications.SendManyAsync(notifs);
            }
            catch
            {
                // Notification failure must never fail the submission response
            }
        }

        private static string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 1)   return "Just now";
            if (diff.TotalMinutes < 60)  return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours   < 24)  return $"{(int)diff.TotalHours} hr ago";
            if (diff.TotalDays    < 2)   return "Yesterday";
            return dt.ToString("MMM d");
        }

        // ═══════════════════════════════════════════════════════════
        // Registration endpoints — backed by live RegistrationSettings
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/student/registration/status
        /// Returns real-time registration status from the same settings that admin manages.
        /// </summary>
        [HttpGet("registration/status")]
        public async Task<IActionResult> GetRegistrationStatus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.GetRegistrationStatusAsync(userId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// GET /api/student/registration/courses
        /// Returns courses available for registration based on live admin settings.
        /// </summary>
        [HttpGet("registration/courses")]
        public async Task<IActionResult> GetRegistrationCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.GetAvailableCoursesAsync(userId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// POST /api/student/registration/courses
        /// Register for a course
        /// </summary>
        [HttpPost("registration/courses")]
        public async Task<IActionResult> RegisterCourses(
            [FromBody] RegisterCourseDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.RegisterCourseAsync(userId, dto.CourseCode);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// DELETE /api/student/registration/courses/{courseCode}
        /// Drop a registered course
        /// </summary>
        [HttpDelete("registration/courses/{courseCode}")]
        public async Task<IActionResult> DropCourse(string courseCode)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            await _registrationService.DropCourseAsync(userId, courseCode);
            return Ok(new { success = true, data = new { message = $"Course '{courseCode}' dropped successfully." } });
        }

        // ═══════════════════════════════════════════════════════════
        // Compatibility routes — same service logic, alternate paths
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/student/courses/available — alias for registration/courses</summary>
        [HttpGet("courses/available")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.GetAvailableCoursesAsync(userId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>POST /api/student/courses/register — alias for registration/courses</summary>
        [HttpPost("courses/register")]
        public async Task<IActionResult> RegisterCourseAlt([FromBody] RegisterCourseDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _registrationService.RegisterCourseAsync(userId, dto.CourseCode);
            return Ok(new { success = true, data = result });
        }

        /// <summary>DELETE /api/student/courses/{code}/drop — alias for registration/courses/{code}</summary>
        [HttpDelete("courses/{code}/drop")]
        public async Task<IActionResult> DropCourseAlt(string code)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            await _registrationService.DropCourseAsync(userId, code);
            return Ok(new { success = true, data = new { message = $"Course '{code}' dropped successfully." } });
        }

        /// <summary>
        /// GET /api/student/final-grades
        /// Returns the student's own published final grades for currently registered courses.
        /// ONLY returns records where Published == true — unpublished grades are never sent.
        /// </summary>
        [HttpGet("final-grades")]
        public async Task<IActionResult> GetPublishedFinalGrades()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var regs = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            var active = (regs ?? Enumerable.Empty<Registration>())
                .Where(r => (r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending)
                            && !r.IsEquivalency
                            && !r.IsArchived)
                .ToList();

            var result = new List<object>();

            foreach (var reg in active)
            {
                var fg = await _unitOfWork.FinalGrades.GetAsync(userId, reg.CourseId);
                // Guard: only published grades are visible to students
                if (fg == null || !fg.Published) continue;

                var course = await _unitOfWork.Courses.GetByIdAsync(reg.CourseId);
                if (course == null) continue;

                decimal total;
                decimal cwTotal;
                if (fg.AdminFinalTotal.HasValue)
                {
                    // Admin entered total directly (via Academic Setup) — use it as-is
                    total   = fg.AdminFinalTotal.Value;
                    cwTotal = total; // no component breakdown available in this path
                }
                else
                {
                    // Compute from components (instructor flow)
                    var midterm  = await _unitOfWork.MidtermGrades.GetAsync(userId, reg.CourseId);
                    var midGrade = midterm?.Grade ?? 0;

                    decimal quizScore = 0;
                    foreach (var q in (await _unitOfWork.Quizzes.GetQuizzesByCourseId(reg.CourseId)).ToList())
                    {
                        var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(q.Id, userId);
                        if (attempt != null) quizScore += attempt.Score;
                    }

                    decimal asnScore = 0;
                    foreach (var a in (await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(reg.CourseId)).ToList())
                    {
                        var sub = await _unitOfWork.Assignments.GetStudentSubmissionAsync(a.Id, userId);
                        if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                            asnScore += sub.Grade ?? 0;
                    }

                    cwTotal = Math.Min(40m, midGrade + quizScore + asnScore + fg.Bonus);
                    total   = cwTotal + fg.FinalScore;
                }

                result.Add(new
                {
                    courseId        = course.Id,
                    courseCode      = course.Code,
                    courseName      = course.Name,
                    finalScore      = fg.AdminFinalTotal.HasValue ? (int?)null : fg.FinalScore,
                    courseworkTotal = fg.AdminFinalTotal.HasValue ? (decimal?)null : Math.Round(cwTotal, 1),
                    total           = Math.Round(total, 1),
                    letterGrade     = FinalGradeToLetter((int)Math.Round(total)),
                });
            }

            return Ok(new { success = true, data = result });
        }

        private static string FinalGradeToLetter(int total) => total switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _     => "F",
        };

        /// <summary>
        /// GET /api/student/transcript
        /// Returns the logged-in student's completed academic transcript —
        /// ONLY courses that have real admin-assigned grades (equivalency
        /// registrations with IsPassed=true and a numeric total).
        /// Shape: { student, completedCourses: [{courseCode, name, credits, year, semester, total, grade, gpaPoints}] }
        /// </summary>
        [HttpGet("transcript")]
        public async Task<IActionResult> GetTranscript()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var result = await _adminService.GetStudentTranscriptAsync(userId);
            return Ok(new { success = true, data = result });
        }
    }
}
