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
    /// Admin Academic Year Reset endpoints.
    /// Two-step flow: preview (no mutation) and execute (mutates inside a single transaction).
    /// Execute requires the configured confirmation text + reset password.
    /// </summary>
    [ApiController]
    [Route("api/admin/academic-reset")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class AcademicYearResetController : ControllerBase
    {
        private readonly IAcademicYearResetService _service;

        public AcademicYearResetController(IAcademicYearResetService service)
        {
            _service = service;
        }

        // ══════════════════════════════════════════════════════════════════
        // POST /api/admin/academic-reset/preview
        // Read-only impact preview for the selected students.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] AcademicYearResetPreviewRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, error = new { code = "BAD_REQUEST", message = "Body required." } });

            if (!request.SelectAll && (request.StudentIds == null || request.StudentIds.Count == 0))
                return BadRequest(new { success = false, error = new { code = "NO_SELECTION", message = "Select at least one student or pass selectAll=true." } });

            var data = await _service.PreviewAsync(request);
            return Ok(new { success = true, data });
        }

        // ══════════════════════════════════════════════════════════════════
        // POST /api/admin/academic-reset/execute
        // Mutates state. Requires confirmationText=RESET and the reset password.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] AcademicYearResetExecuteRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, error = new { code = "BAD_REQUEST", message = "Body required." } });

            if (!request.SelectAll && (request.StudentIds == null || request.StudentIds.Count == 0))
                return BadRequest(new { success = false, error = new { code = "NO_SELECTION", message = "Select at least one student or pass selectAll=true." } });

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            try
            {
                var data = await _service.ExecuteAsync(adminId, request);
                return Ok(new { success = true, data });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { success = false, error = new { code = "RESET_BLOCKED", message = ex.Message } });
            }
        }
    }
}
