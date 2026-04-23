using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Abstraction.Contracts;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Abstractions.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Presistence;
using Shared.Dtos.Auth_Module;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class AuthenticationController(
        IServiceManager _serviceManager,
        UserManager<User> _userManager,
        UniversityDbContext _dbContext,
        IEmailService _emailService,
        INotificationService _notificationService) : ControllerBase
    {

        // Post =>  Register 
        [HttpPost("Register")]

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResultDto>> RegisterAsync(RegisterDto registerDto, string role = "Student")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            var userResult = await _serviceManager.AuthenticationService.RegisterAsync(registerDto, role);

            return Ok(userResult);
        }

        [HttpPost("register-student/{departmentId}/department")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResultDto>> RegisterStudentAsync(int departmentId, RegisterStudentDto registerStudentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            var userResult = await _serviceManager.AuthenticationService.RegisterStudentAsync(departmentId, registerStudentDto);

            return Ok(userResult);
        }

                // POST => Login — returns { token, user } for the uni-learn React frontend
        [HttpPost("Login")]
        public async Task<ActionResult<FrontendLoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var userResult = await _serviceManager.AuthenticationService.LoginAsync(loginDto);
            // Wrap in { success, data: { token, user } } shape expected by the frontend
            return Ok(FrontendLoginResponseDto.FromUserResult(userResult));
        }


        [HttpPut("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPasswordByAdmin(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var result = await _serviceManager.AuthenticationService.ResetPasswordAsync(resetPasswordDto.Email , resetPasswordDto.NewPassword);
            return Ok(result);
        }

        /// <summary>
        /// POST /api/authentication/logout
        /// Invalidate token server-side (add to blocklist/revoke list)
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

        /// <summary>
        /// POST /api/authentication/refresh
        /// Exchange current JWT for a new one
        /// </summary>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken()
        {
            // TODO: Implement token refresh
            return Ok(new RefreshTokenResponseDto { Token = string.Empty });
        }

        /// <summary>
        /// POST /api/authentication/change-password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false,
                    error = new { code = "VALIDATION_ERROR", message = "Validation failed." } });

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { success = false,
                    error = new { code = "PASSWORD_MISMATCH", message = "Passwords do not match." } });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { success = false,
                    error = new { code = "INVALID_PASSWORD",
                        message = result.Errors.FirstOrDefault()?.Description ?? "Failed to change password." } });

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            // Notify the user their password was changed
            try
            {
                var now = DateTime.UtcNow;
                await _notificationService.SendAsync(new Notification
                {
                    UserId = userId,
                    Type   = "password_changed",
                    Title  = "Password Changed 🔒",
                    Body   = $"Your password was successfully changed on {now:MMM d} at {now:h:mm tt} UTC.",
                    IsRead = false,
                });
            }
            catch { /* never block the response */ }

            return Ok(new { success = true, message = "Password changed successfully. Please log in again." });
        }

        /// <summary>
        /// POST /api/authentication/forgot-password/request
        /// </summary>
        [HttpPost("forgot-password/request")]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new {
                    success = false,
                    error = new { code = "USER_NOT_FOUND",
                        message = "No account found with this email address." }
                });

            if (string.IsNullOrWhiteSpace(user.SubEmail))
                return BadRequest(new {
                    success = false,
                    error = new { code = "NO_RECOVERY_EMAIL",
                        message = "No recovery email linked to this account. Please contact your administrator." }
                });

            // Invalidate old unused OTPs
            var oldOtps = _dbContext.PasswordResetOtps
                .Where(o => o.Email == dto.Email && !o.IsUsed);
            _dbContext.PasswordResetOtps.RemoveRange(oldOtps);

            // Generate 6-digit OTP
            var code = new Random().Next(100000, 999999).ToString();

            var otp = new PasswordResetOtp
            {
                Email     = dto.Email,
                Code      = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Attempts  = 0,
                IsUsed    = false
            };
            _dbContext.PasswordResetOtps.Add(otp);
            await _dbContext.SaveChangesAsync();

            var emailBody = BuildOtpEmail(user.DisplayName, code);
            var emailResult = await _emailService.SendEmailWithResultAsync(
                user.SubEmail,
                "Password Reset Code - Akhbar Alyoum Academy",
                emailBody);

            if (!emailResult.Succeeded)
            {
                return StatusCode(502, new {
                    success = false,
                    error = new { code = "EMAIL_DELIVERY_FAILED",
                        message = "Failed to send verification code. Please try again later or contact your administrator.",
                        provider = emailResult.Provider,
                        detail = emailResult.ErrorMessage }
                });
            }

            return Ok(new {
                success = true,
                message = "A 6-digit verification code has been sent to your recovery email."
            });
        }

        /// <summary>
        /// POST /api/authentication/forgot-password/verify
        /// </summary>
        [HttpPost("forgot-password/verify")]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] ForgotPasswordVerifyDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new {
                    success = false,
                    error = new { code = "PASSWORD_MISMATCH", message = "Passwords do not match." }
                });

            var otp = _dbContext.PasswordResetOtps
                .Where(o => o.Email == dto.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (otp == null)
                return BadRequest(new {
                    success = false,
                    error = new { code = "OTP_EXPIRED",
                        message = "Code expired or not found. Please request a new one." }
                });

            if (otp.Attempts >= 3)
                return BadRequest(new {
                    success = false,
                    error = new { code = "MAX_ATTEMPTS",
                        message = "Maximum attempts reached. Please wait 60 seconds and request a new code." }
                });

            if (otp.Code != dto.Code)
            {
                otp.Attempts++;
                await _dbContext.SaveChangesAsync();
                int remaining = 3 - otp.Attempts;
                return BadRequest(new {
                    success = false,
                    error = new {
                        code = "INVALID_CODE",
                        message = remaining > 0
                            ? $"Invalid code. {remaining} attempt(s) remaining."
                            : "Maximum attempts reached. Please wait 60 seconds and request a new code.",
                        attemptsRemaining = remaining
                    }
                });
            }

            otp.IsUsed = true;
            await _dbContext.SaveChangesAsync();

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new {
                    success = false,
                    error = new { code = "PASSWORD_RESET_FAILED",
                        message = result.Errors.FirstOrDefault()?.Description ?? "Failed to reset password." }
                });

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            return Ok(new {
                success = true,
                message = "Password reset successfully. You can now log in with your new password."
            });
        }

        private static string BuildOtpEmail(string name, string code) => $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1.0' />
  <style>
    body {{ margin:0; padding:0; background:#f4f4f4; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; background:#f4f4f4; padding:24px 0; }}
    .card {{ max-width:520px; width:92%; margin:0 auto; background:#fff;
             border-radius:16px; overflow:hidden; box-shadow:0 4px 24px rgba(0,0,0,.08); }}
    .header {{ background:linear-gradient(135deg,#4338ca,#6366f1);
               padding:28px 24px; text-align:center; }}
    .header-icon {{ font-size:36px; margin-bottom:8px; }}
    .header-title {{ color:#fff; margin:0; font-size:20px; font-weight:900; }}
    .body {{ padding:28px 24px; }}
    .greeting {{ color:#374151; font-size:15px; margin:0 0 10px; }}
    .subtitle {{ color:#374151; font-size:14px; margin:0 0 24px; line-height:1.6; }}
    .otp-wrap {{ text-align:center; margin:24px 0; }}
    .otp-box {{ display:inline-block; background:#f8f9ff; border:2px dashed #6366f1;
                border-radius:14px; padding:16px 28px; }}
    .otp-code {{ margin:0; font-size:32px; font-weight:900; letter-spacing:8px; color:#4338ca; }}
    .warning {{ background:#fef3c7; border:1.5px solid #fcd34d;
                border-radius:10px; padding:12px 16px; }}
    .warning-text {{ margin:0; color:#92400e; font-size:13px; line-height:1.5; }}
    .footer {{ background:#f9fafb; padding:14px; text-align:center;
               border-top:1px solid #e5e7eb; }}
    .footer-text {{ margin:0; color:#9ca3af; font-size:11px; }}
    @media only screen and (max-width: 480px) {{
      .wrapper {{ padding:16px 0; }}
      .header {{ padding:22px 16px; }}
      .header-icon {{ font-size:30px; }}
      .header-title {{ font-size:17px; }}
      .body {{ padding:20px 16px; }}
      .otp-box {{ padding:14px 18px; }}
      .otp-code {{ font-size:26px; letter-spacing:6px; }}
      .greeting, .subtitle {{ font-size:14px; }}
    }}
  </style>
</head>
<body>
  <div class='wrapper'>
    <div class='card'>
      <div class='header'>
        <div class='header-icon'>🔑</div>
        <h1 class='header-title'>Password Reset Code</h1>
      </div>
      <div class='body'>
        <p class='greeting'>Hello <strong>{name}</strong>,</p>
        <p class='subtitle'>
          Use the code below to reset your password.
          This code expires in <strong>10 minutes</strong>.
        </p>
        <div class='otp-wrap'>
          <div class='otp-box'>
            <p class='otp-code'>{code}</p>
          </div>
        </div>
        <div class='warning'>
          <p class='warning-text'>
            ⚠️ <strong>Important:</strong> You have 3 attempts. Do not share this code with anyone.
          </p>
        </div>
      </div>
      <div class='footer'>
        <p class='footer-text'>© Akhbar Alyoum Academy — University Management System</p>
      </div>
    </div>
  </div>
</body>
</html>";

    }
}
