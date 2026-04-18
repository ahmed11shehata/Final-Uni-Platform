using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Shared.Dtos.Auth_Module;
using Shared.Dtos.Info_Module.UserDtos;
using AYA_UIS.Core.Abstractions.Contracts;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/user")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class UserController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public UserController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // ─── EXISTING ENDPOINTS ─────────────────────────────────────────────

        [HttpGet("{academicCode}/academic")]
        public async Task<IActionResult> GetAcademicInfo(string academicCode)
        {
            var userProfile = await _serviceManager.UserService.GetUserProfileByAcademicCodeAsync(academicCode);
            return Ok(userProfile);
        }

        [HttpPatch("update-profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto updateProfilePictureDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            await _serviceManager.UserService.UpdateProfilePictureAsync(userId, updateProfilePictureDto);
            return NoContent();
        }

        [HttpPatch("update-student-specialization")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudentSpecialization([FromBody] UpdateStudentSpecializationDto updateStudentSpecializationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            await _serviceManager.UserService.UpdateStudentSpecializationAsync(userId, updateStudentSpecializationDto);
            return NoContent();
        }

        // ─── NEW ENDPOINTS (Auth & User Profile Module) ─────────────────────

        /// <summary>
        /// GET /api/user/profile
        /// Returns full profile for the current authenticated user.
        /// Response shape: { success, data: { user: { id, name, email, role, department, phone, address, dob, gender, avatar, themeId, [year], [entryYear], [gpa] } } }
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var profile = await _serviceManager.UserService.GetUserProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// PUT /api/user/profile
        /// Update editable profile fields (name, phone, address, dob, gender).
        /// Rejects email, role, department with 400.
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "Validation failed.",
                        details = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    }
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var updated = await _serviceManager.UserService.UpdateProfileAsync(userId, dto);
            return Ok(updated);
        }

        /// <summary>
        /// POST /api/user/avatar
        /// Upload user avatar image. multipart/form-data, field "file".
        /// Image files only (jpg, jpeg, png, webp). Max 5MB.
        /// </summary>
        [HttpPost("avatar")]
        [Authorize]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "Validation failed.",
                        details = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    }
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            var avatarUrl = await _serviceManager.UserService.UploadAvatarAsync(userId, dto);

            return Ok(new
            {
                success = true,
                data = new
                {
                    avatar = avatarUrl
                }
            });
        }

        /// <summary>
        /// PUT /api/user/theme
        /// Update user theme preference.
        /// Valid values: default | ramadan | space | arctic | pharaoh | saladin | fog
        /// Returns 400 if value not in this list.
        /// </summary>
        [HttpPut("theme")]
        [Authorize]
        public async Task<IActionResult> UpdateTheme([FromBody] UpdateThemeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "Validation failed.",
                        details = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    }
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, error = new { code = "UNAUTHORIZED", message = "Not authenticated." } });

            await _serviceManager.UserService.UpdateThemeAsync(userId, dto);

            return Ok(new
            {
                success = true,
                data = new
                {
                    themeId = dto.ThemeId
                }
            });
        }
    }
}