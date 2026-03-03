using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AYA_UIS.Application.Contracts;
using Shared.Dtos.Auth_Module;
using AYA_UIS.Core.Abstractions.Contracts;

namespace AYA_UIS.Core.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name! });
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return null;
            
            return new RoleDto { Id = role.Id, Name = role.Name! };
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return null;
            
            return new RoleDto { Id = role.Id, Name = role.Name! };
        }

        public async Task<IdentityResult> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            var roleExists = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);
            if (roleExists)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Description = $"Role '{createRoleDto.RoleName}' already exists." 
                });
            }

            var role = new IdentityRole(createRoleDto.RoleName);
            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(string roleId, UpdateRoleDto updateRoleDto)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Description = $"Role with ID '{roleId}' not found." 
                });
            }

            role.Name = updateRoleDto.NewRoleName;
            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Description = $"Role with ID '{roleId}' not found." 
                });
            }

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IdentityResult> UpdateUserRoleByEmailAsync(UpdateUserRoleByEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User with email '{dto.Email}' not found."
                });
            }

            if (!await _roleManager.RoleExistsAsync(dto.NewRoleName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Role '{dto.NewRoleName}' does not exist."
                });
            }

            // 1️⃣ remove all current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // 2️⃣ add new role
            return await _userManager.AddToRoleAsync(user, dto.NewRoleName);
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with ID '{userId}' not found.");

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> UpdateUserRoleByAcademicCodeAsync(UpdateUserRoleDto dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == dto.AcademicCode);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User with academic code '{dto.AcademicCode}' not found."
                });
            }

            if (!await _roleManager.RoleExistsAsync(dto.NewRoleName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Role '{dto.NewRoleName}' does not exist."
                });
            }

            // 1️⃣ remove all current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // 2️⃣ add new role
            return await _userManager.AddToRoleAsync(user, dto.NewRoleName);
        }

        public async Task<UserRoleInfoDto> GetUserRoleInfoByAcademicCodeAsync(string academicCode)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == academicCode);

            if (user == null)
                throw new NotFoundException(
                    $"User with academic code '{academicCode}' not found."
                );

            var roles = await _userManager.GetRolesAsync(user);

            return new UserRoleInfoDto
            {
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = roles
            };
        }


    }
}
