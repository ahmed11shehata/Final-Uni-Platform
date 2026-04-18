using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator        _mediator;
        private readonly UserManager<User> _userManager;

        public AdminDashboardController(
            IMediator        mediator,
            UserManager<User> userManager)
        {
            _mediator    = mediator;
            _userManager = userManager;
        }

        /// <summary>Admin dashboard summary statistics.</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetAdminDashboardQuery());
            return Ok(result);
        }

        /// <summary>List all users with optional search & role filter.</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role)
        {
            var result = await _mediator.Send(new GetAdminUsersQuery(search, role));
            return Ok(result);
        }

        /// <summary>Get a single user by academic code.</summary>
        [HttpGet("users/{academicCode}")]
        public async Task<IActionResult> GetUser(string academicCode)
        {
            if (string.IsNullOrWhiteSpace(academicCode))
                return BadRequest(new { message = "Academic code is required." });

            var result = await _mediator.Send(new GetAdminUserByCodeQuery(academicCode));
            if (result is null)
                return NotFound(new { message = $"User '{academicCode}' not found." });

            return Ok(result);
        }

        /// <summary>Update a user's allowed credits or active status.</summary>
        [HttpPut("users/{academicCode}")]
        public async Task<IActionResult> UpdateUser(
            string              academicCode,
            [FromBody] AdminUpdateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(academicCode))
                return BadRequest(new { message = "Academic code is required." });

            if (dto is null)
                return BadRequest(new { message = "Request body is required." });

            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Academic_Code == academicCode);

            if (user is null)
                return NotFound(new { message = $"User '{academicCode}' not found." });

            if (dto.AdminMaxCredits.HasValue)
            {
                if (dto.AdminMaxCredits.Value < 0 || dto.AdminMaxCredits.Value > 30)
                    return BadRequest(new { message = "Allowed credits must be between 0 and 30." });

                user.AllowedCredits = dto.AdminMaxCredits.Value;
            }

            if (dto.Active.HasValue)
            {
                if (!dto.Active.Value)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                }
                else
                {
                    await _userManager.SetLockoutEnabledAsync(user, false);
                    await _userManager.SetLockoutEndDateAsync(user, null);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "User updated successfully." });
        }

        /// <summary>Delete a user by academic code.</summary>
        [HttpDelete("users/{academicCode}")]
        public async Task<IActionResult> DeleteUser(string academicCode)
        {
            if (string.IsNullOrWhiteSpace(academicCode))
                return BadRequest(new { message = "Academic code is required." });

            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Academic_Code == academicCode);

            if (user is null)
                return NotFound(new { message = $"User '{academicCode}' not found." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = $"User '{academicCode}' deleted." });
        }
    }

    public class AdminUpdateUserDto
    {
        public int?  AdminMaxCredits { get; set; }
        public bool? Active          { get; set; }
    }
}
