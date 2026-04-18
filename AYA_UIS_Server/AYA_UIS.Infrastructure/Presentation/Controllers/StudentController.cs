using System.Security.Claims;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Abstractions.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
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

        public StudentController(
            IStudentRegistrationService registrationService,
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            IAdminService adminService)
        {
            _registrationService = registrationService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _adminService = adminService;
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

        /// <summary>
        /// POST /api/student/assignments/{assignmentId}/submit
        /// Submit an assignment
        /// </summary>
        [HttpPost("assignments/{assignmentId}/submit")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<SubmissionResponseDto>> SubmitAssignment(
            string assignmentId,
            [FromForm] SubmitAssignmentFormDto dto)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new SubmissionResponseDto() });
        }

        /// <summary>
        /// POST /api/student/quizzes/{quizId}/submit
        /// Submit a quiz
        /// </summary>
        [HttpPost("quizzes/{quizId}/submit")]
        public async Task<ActionResult<SubmissionResponseDto>> SubmitQuiz(
            string quizId,
            [FromBody] SubmitQuizDto dto)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new SubmissionResponseDto() });
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
