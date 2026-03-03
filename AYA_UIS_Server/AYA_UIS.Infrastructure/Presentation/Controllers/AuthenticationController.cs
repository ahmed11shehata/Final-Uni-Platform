using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/[controller]")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class AuthenticationController (IServiceManager _serviceManager):ControllerBase
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

        // Post = >  Login 
        [HttpPost("Login")]
        public async Task<ActionResult<UserResultDto>> LoginAsync(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var userResult = await _serviceManager.AuthenticationService.LoginAsync(loginDto);
            return Ok(userResult);
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

    }
}
