using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using AYA_UIS.Application.Contracts;
using Shared.Dtos.Auth_Module;
using AYA_UIS.Core.Abstractions.Contracts;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public RolesController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _serviceManager.RoleService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{roleId}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<RoleDto>> GetRoleById(string roleId)
        {
            var role = await _serviceManager.RoleService.GetRoleByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = $"Role with ID '{roleId}' not found." });

            return Ok(role);
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        [HttpGet("by-name/{roleName}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<RoleDto>> GetRoleByName(string roleName)
        {
            var role = await _serviceManager.RoleService.GetRoleByNameAsync(roleName);
            if (role == null)
                return NotFound(new { message = $"Role '{roleName}' not found." });

            return Ok(role);
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            var result = await _serviceManager.RoleService.CreateRoleAsync(createRoleDto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            var createdRole = await _serviceManager.RoleService.GetRoleByNameAsync(createRoleDto.RoleName);
            return CreatedAtAction(nameof(GetRoleById), new { roleId = createdRole!.Id }, createdRole);
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        [HttpPut("{roleId}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleDto updateRoleDto)
        {
            var result = await _serviceManager.RoleService.UpdateRoleAsync(roleId, updateRoleDto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return NoContent();
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        [HttpDelete("{roleId}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var result = await _serviceManager.RoleService.DeleteRoleAsync(roleId);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return NoContent();
        }

        /// <summary>
        /// Update user role by email
        /// </summary>
        [HttpPut("update-user-role-by-email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRoleByEmail([FromBody] UpdateUserRoleByEmailDto dto)
        {
            var result = await _serviceManager.RoleService
                .UpdateUserRoleByEmailAsync(dto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new
            {
                message = $"User role updated successfully to '{dto.NewRoleName}'."
            });
        }



        /// <summary>
        /// Update user role by academic code
        /// </summary>
        [HttpPut("update-user-role{academicCode}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDto dto)
        {
            var result = await _serviceManager.RoleService
                .UpdateUserRoleByAcademicCodeAsync(dto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new
            {
                message = $"User role updated successfully to '{dto.NewRoleName}'."
            });
        }

        /// <summary>
        /// Get user role info by academic code
        /// </summary>
        [HttpGet("user-role-info/{academicCode}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserRoleInfoDto>> GetUserRoleInfoByAcademicCode(string academicCode)
        {
            try
            {
                var result = await _serviceManager.RoleService
                    .GetUserRoleInfoByAcademicCodeAsync(academicCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


    }
}

