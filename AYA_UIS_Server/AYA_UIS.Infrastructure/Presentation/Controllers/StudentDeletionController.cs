using System.Security.Claims;
using Abstraction.Contracts;
using AYA_UIS.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Admin_Module;

namespace Presentation.Controllers
{
    /// <summary>
    /// Permanent student delete (Email Manager → Danger Zone).
    /// Two-step flow: preview (read-only) and execute (transactional).
    /// Execute requires the fixed password "StudentDelete@123#" and a re-typed
    /// academic code. Frontend validation is advisory; the backend is authoritative.
    /// </summary>
    [ApiController]
    [Route("api/admin/student-delete")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class StudentDeletionController : ControllerBase
    {
        private readonly IStudentDeletionService _service;

        public StudentDeletionController(IStudentDeletionService service)
        {
            _service = service;
        }

        // ══════════════════════════════════════════════════════════════════
        // GET /api/admin/student-delete/preview/{academicCode}
        // ══════════════════════════════════════════════════════════════════
        [HttpGet("preview/{academicCode}")]
        public async Task<IActionResult> Preview(string academicCode)
        {
            try
            {
                var data = await _service.PreviewAsync(academicCode);
                return Ok(new { success = true, data });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, error = new { code = "STUDENT_NOT_FOUND", message = ex.Message } });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { success = false, error = new { code = "BAD_REQUEST", message = ex.Message } });
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // POST /api/admin/student-delete/execute
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] StudentDeletionExecuteRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, error = new { code = "BAD_REQUEST", message = "Body required." } });

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            try
            {
                var data = await _service.ExecuteAsync(adminId, request);
                return Ok(new { success = true, data });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, error = new { code = "STUDENT_NOT_FOUND", message = ex.Message } });
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN", message = ex.Message } });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { success = false, error = new { code = "DELETE_BLOCKED", message = ex.Message } });
            }
        }
    }
}
