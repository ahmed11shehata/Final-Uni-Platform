using System.Security.Claims;
using AYA_UIS.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Admin_Module;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ═══════════════════════════════════════════════════════════
        // 3.1 Dashboard Stats
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/stats</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetStatsAsync();
            return Ok(new { success = true, data = stats });
        }

        // ═══════════════════════════════════════════════════════════
        // 3.2 Email Manager
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/emails — list all accounts with counts</summary>
        [HttpGet("emails")]
        public async Task<IActionResult> GetEmails()
        {
            var result = await _adminService.GetEmailsAsync();
            return Ok(new { success = true, data = result });
        }

        /// <summary>POST /api/admin/emails/create — create new email account</summary>
        [HttpPost("emails/create")]
        public async Task<IActionResult> CreateEmailAccount([FromBody] CreateEmailAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            var result = await _adminService.CreateEmailAccountAsync(dto);
            return StatusCode(201, new { success = true, data = result });
        }

        /// <summary>PUT /api/admin/emails/:id/toggle-active</summary>
        [HttpPut("emails/{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _adminService.ToggleActiveAsync(id, currentUserId!);
            return Ok(new { success = true, data = result });
        }

        /// <summary>PUT /api/admin/emails/:id/reset-password</summary>
        [HttpPut("emails/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] AdminResetPasswordBodyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var result = await _adminService.ResetPasswordAsync(id, dto.NewPassword);
            return Ok(new { success = true, data = result });
        }

        /// <summary>PUT /api/admin/emails/:id</summary>
        [HttpPut("emails/{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] UpdateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed." } });

            var result = await _adminService.UpdateAccountAsync(id, dto);
            return Ok(new { success = true, data = result });
        }

        /// <summary>DELETE /api/admin/emails/:id</summary>
        [HttpDelete("emails/{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _adminService.DeleteAccountAsync(id, currentUserId);
            return Ok(new { success = true, message = "Account deleted successfully." });
        }

        // ═══════════════════════════════════════════════════════════
        // 3.3 Schedule Manager
        // ═══════════════════════════════════════════════════════════

        /// <summary>POST /api/admin/schedule/sessions</summary>
        [HttpPost("schedule/sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            var result = await _adminService.CreateSessionAsync(dto);
            return StatusCode(201, new { success = true, data = result });
        }

        /// <summary>DELETE /api/admin/schedule/sessions/:id</summary>
        [HttpDelete("schedule/sessions/{id:int}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            await _adminService.DeleteSessionAsync(id);
            return Ok(new { success = true, message = "Session deleted." });
        }

        /// <summary>POST /api/admin/schedule/exams</summary>
        [HttpPost("schedule/exams")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            var result = await _adminService.CreateExamAsync(dto);
            return StatusCode(201, new { success = true, data = result });
        }

        /// <summary>DELETE /api/admin/schedule/exams/:id</summary>
        [HttpDelete("schedule/exams/{id:int}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            await _adminService.DeleteExamAsync(id);
            return Ok(new { success = true, message = "Exam deleted." });
        }

        /// <summary>POST /api/admin/schedule/publish — mark schedule as published</summary>
        [HttpPost("schedule/publish")]
        public async Task<IActionResult> PublishSchedule()
        {
            var publishedAt = await _adminService.PublishScheduleAsync();
            return Ok(new { success = true, data = new { publishedAt } });
        }

        /// <summary>GET /api/admin/schedule?year=1&amp;group=A&amp;view=weekly|exam</summary>
        [HttpGet("schedule")]
        public async Task<IActionResult> GetSchedule(
            [FromQuery] int? year,
            [FromQuery] string? group,
            [FromQuery] string? view)
        {
            var result = await _adminService.GetScheduleAsync(year, group, view);
            return Ok(new { success = true, data = result });
        }

        // ═══════════════════════════════════════════════════════════
        // 3.4 Registration Manager
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/registration/status</summary>
        [HttpGet("registration/status")]
        public async Task<IActionResult> GetRegistrationStatus()
        {
            var status = await _adminService.GetRegistrationStatusAsync();
            return Ok(new { success = true, data = status });
        }

        /// <summary>POST /api/admin/registration/start</summary>
        [HttpPost("registration/start")]
        public async Task<IActionResult> StartRegistration([FromBody] StartRegistrationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            var result = await _adminService.StartRegistrationAsync(dto);
            return StatusCode(201, new { success = true, data = result });
        }

        /// <summary>POST /api/admin/registration/stop</summary>
        [HttpPost("registration/stop")]
        public async Task<IActionResult> StopRegistration()
        {
            var closedAt = await _adminService.StopRegistrationAsync();
            return Ok(new { success = true, data = new { closedAt } });
        }

        /// <summary>PUT /api/admin/registration/settings</summary>
        [HttpPut("registration/settings")]
        public async Task<IActionResult> UpdateRegistrationSettings([FromBody] StartRegistrationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            var result = await _adminService.UpdateRegistrationSettingsAsync(dto);
            return Ok(new { success = true, data = result });
        }

        // ═══════════════════════════════════════════════════════════
        // 3.5 Courses Manager
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/courses</summary>
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses(
            [FromQuery] int? year,
            [FromQuery] string? semester,
            [FromQuery] string? type,
            [FromQuery] string? search)
        {
            var courses = await _adminService.GetCoursesAsync(year, semester, type, search);
            return Ok(new { success = true, data = courses });
        }

        /// <summary>PUT /api/admin/courses/settings</summary>
        [HttpPut("courses/settings")]
        public async Task<IActionResult> UpdateCourseSettings([FromBody] AdminCourseSettingsDto dto)
        {
            await _adminService.UpdateCourseSettingsAsync(dto);
            return Ok(new { success = true, message = "Course settings updated." });
        }

        // ═══════════════════════════════════════════════════════════
        // 3.6 Student Control
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/student/:studentId</summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudent(string studentId)
        {
            var result = await _adminService.GetStudentAsync(studentId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>POST /api/admin/student/:studentId/courses/add</summary>
        [HttpPost("student/{studentId}/courses/add")]
        public async Task<IActionResult> ForceAddCourse(string studentId, [FromBody] AdminAddCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            await _adminService.ForceAddCourseAsync(studentId, dto);
            return Ok(new { success = true, message = "Course added successfully." });
        }

        /// <summary>DELETE /api/admin/student/:studentId/courses/:code</summary>
        [HttpDelete("student/{studentId}/courses/{code}")]
        public async Task<IActionResult> ForceRemoveCourse(string studentId, string code)
        {
            await _adminService.ForceRemoveCourseAsync(studentId, code);
            return Ok(new { success = true, message = "Course removed." });
        }

        /// <summary>PUT /api/admin/student/:studentId/unlock/:code</summary>
        [HttpPut("student/{studentId}/unlock/{code}")]
        public async Task<IActionResult> UnlockCourse(string studentId, string code)
        {
            var message = await _adminService.UnlockCourseAsync(studentId, code);
            return Ok(new { success = true, message });
        }

        /// <summary>PUT /api/admin/student/:studentId/lock/:code?reason=...</summary>
        [HttpPut("student/{studentId}/lock/{code}")]
        public async Task<IActionResult> LockCourse(string studentId, string code, [FromQuery] string? reason = null)
        {
            await _adminService.LockCourseAsync(studentId, code, reason);
            return Ok(new { success = true, message = "Course locked." });
        }

        /// <summary>PUT /api/admin/student/:studentId/max-credits</summary>
        [HttpPut("student/{studentId}/max-credits")]
        public async Task<IActionResult> OverrideMaxCredits(
            string studentId,
            [FromBody] AdminMaxCreditsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            await _adminService.OverrideMaxCreditsAsync(studentId, dto);
            return Ok(new { success = true, message = "Max credits updated." });
        }

        // ── 3.7 Academic Setup ────────────────────────────────────

        /// <summary>GET /api/admin/student/:studentId/academic-setup</summary>
        [HttpGet("student/{studentId}/academic-setup")]
        public async Task<IActionResult> GetAcademicSetup(string studentId)
        {
            var result = await _adminService.GetAcademicSetupAsync(studentId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>PUT /api/admin/student/:studentId/academic-setup</summary>
        [HttpPut("student/{studentId}/academic-setup")]
        public async Task<IActionResult> SaveAcademicSetup(
            string studentId,
            [FromBody] AcademicSetupSaveRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed.",
                        details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
                });

            var result = await _adminService.SaveAcademicSetupAsync(studentId, dto);
            return Ok(new { success = true, data = result, message = "Academic setup saved successfully." });
        }

        // ═══════════════════════════════════════════════════════════
        // 3.9 Instructor Control
        // ═══════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/instructor-control — open courses + all instructors + assignments</summary>
        [HttpGet("instructor-control")]
        public async Task<IActionResult> GetInstructorControl()
        {
            var result = await _adminService.GetInstructorControlAsync();
            return Ok(new { success = true, data = result });
        }

        /// <summary>PUT /api/admin/instructor-control/{courseId} — replace instructor list for course</summary>
        [HttpPut("instructor-control/{courseId:int}")]
        public async Task<IActionResult> AssignInstructors(
            int courseId,
            [FromBody] AssignInstructorsDto dto)
        {
            await _adminService.AssignInstructorsAsync(courseId, dto);
            return Ok(new { success = true, message = "Instructors assigned successfully." });
        }
    }
}
