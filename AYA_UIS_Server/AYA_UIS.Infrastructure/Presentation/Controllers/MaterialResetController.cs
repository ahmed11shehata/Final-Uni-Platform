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
    /// Admin "Reset Material" endpoints.
    /// Two-step flow: preview (no mutation) and execute (single transaction).
    /// Execute requires the fixed password "Material@123#" (overridable via
    /// <c>MaterialReset:Password</c> in configuration).
    /// </summary>
    [ApiController]
    [Route("api/admin/material-reset")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class MaterialResetController : ControllerBase
    {
        private readonly IMaterialResetService _service;

        public MaterialResetController(IMaterialResetService service)
        {
            _service = service;
        }

        // ══════════════════════════════════════════════════════════════════
        // GET /api/admin/material-reset/courses
        // Returns every course in the catalog (Courses table) — including ones
        // with no material — so the admin can pick any of them.
        // ══════════════════════════════════════════════════════════════════
        [HttpGet("courses")]
        public async Task<IActionResult> GetCatalog()
        {
            var data = await _service.GetCatalogAsync();
            return Ok(new { success = true, data });
        }

        // ══════════════════════════════════════════════════════════════════
        // POST /api/admin/material-reset/preview
        // Read-only impact preview. Surfaces pending-submission blockers.
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] MaterialResetPreviewRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, error = new { code = "BAD_REQUEST", message = "Body required." } });

            if (!request.SelectAll && (request.CourseIds == null || request.CourseIds.Count == 0))
                return BadRequest(new { success = false, error = new { code = "NO_SELECTION", message = "Select at least one course or pass selectAll=true." } });

            var data = await _service.PreviewAsync(request);
            return Ok(new { success = true, data });
        }

        // ══════════════════════════════════════════════════════════════════
        // POST /api/admin/material-reset/execute
        // Mutates state. Requires password = "Material@123#" (configurable).
        // ══════════════════════════════════════════════════════════════════
        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] MaterialResetExecuteRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, error = new { code = "BAD_REQUEST", message = "Body required." } });

            if (!request.SelectAll && (request.CourseIds == null || request.CourseIds.Count == 0))
                return BadRequest(new { success = false, error = new { code = "NO_SELECTION", message = "Select at least one course or pass selectAll=true." } });

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            try
            {
                var data = await _service.ExecuteAsync(adminId, request);
                return Ok(new { success = true, data });
            }
            catch (BadRequestException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                // Wrong/empty password — generic message, no length/value hints, no logging.
                return BadRequest(new { success = false, error = new { code = "INVALID_PASSWORD", message = "Incorrect reset password." } });
            }
            catch (BadRequestException ex)
            {
                // Blocked-pending-submissions or empty selection.
                return BadRequest(new { success = false, error = new { code = "RESET_BLOCKED", message = ex.Message } });
            }
        }
    }
}
