using AYA_UIS.Application.Commands.RegistrationSettings;
using AYA_UIS.Application.Queries.RegistrationSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/registration-settings")]
    public class RegistrationSettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RegistrationSettingsController(IMediator mediator)
            => _mediator = mediator;

        /// <summary>
        /// GET current registration window status.
        /// Public endpoint — students need to check this before registering.
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var result = await _mediator.Send(new GetRegistrationStatusQuery());
            return Ok(result);
        }

        /// <summary>
        /// Open the registration window. Admin only.
        /// </summary>
        [HttpPost("open")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Open([FromBody] OpenRegistrationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage) });

            if (dto is null)
                return BadRequest(new { errors = "Request body is required." });

            if (string.IsNullOrWhiteSpace(dto.Semester))
                return BadRequest(new { errors = "Semester is required." });

            if (dto.Deadline == default)
                return BadRequest(new { errors = "Deadline is required." });

            if (dto.Deadline <= DateTime.UtcNow)
                return BadRequest(new { errors = "Deadline must be in the future." });

            var result = await _mediator.Send(new OpenRegistrationCommand(dto));
            return Ok(result);
        }

        /// <summary>
        /// Close the registration window. Admin only.
        /// </summary>
        [HttpPost("close")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Close()
        {
            var result = await _mediator.Send(new CloseRegistrationCommand());
            return Ok(new
            {
                success = result,
                message = result ? "Registration window closed." : "No active registration window to close."
            });
        }
    }
}
