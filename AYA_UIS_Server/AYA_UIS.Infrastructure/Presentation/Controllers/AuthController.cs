using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AYA_UIS.Application.Contracts;
using Shared.Dtos.Auth_Module;
using AYA_UIS.Core.Abstractions.Contracts;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class AuthController(IServiceManager _serviceManager) : ControllerBase
    {

        /// <summary>
        /// POST /api/auth/login
        /// Authenticate user with email, password, and role.
        /// Returns user object + JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<FrontendLoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var userResult = await _serviceManager.AuthenticationService.LoginAsync(loginDto);
            return Ok(FrontendLoginResponseDto.FromUserResult(userResult));
        }

        /// <summary>
        /// GET /api/auth/me
        /// Returns the current authenticated user's info from the JWT.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = "UNAUTHORIZED" });

            var userResult = await _serviceManager.AuthenticationService.GetCurrentUserAsync(userId);
            var response = FrontendLoginResponseDto.FromUserResult(userResult);
            // For /me, we don't return a new token — just the user object
            return Ok(new
            {
                success = true,
                data = new
                {
                    user = response.Data.User
                }
            });
        }

        /// <summary>
        /// POST /api/auth/logout
        /// Invalidate token server-side (add to blocklist).
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    await _serviceManager.AuthenticationService.LogoutAsync(token);
                }
            }

            return Ok(new { success = true, message = "Logged out successfully." });
        }
    }
}
