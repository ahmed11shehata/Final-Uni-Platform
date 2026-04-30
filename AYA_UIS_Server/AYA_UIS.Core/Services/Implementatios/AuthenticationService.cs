using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Common;
using Shared.Dtos.Auth_Module;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;

namespace AYA_UIS.Core.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IOptions<JwtOptions> _options;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenBlocklistService _tokenBlocklist;

        public AuthenticationService(
            UserManager<User> userManager,
            IOptions<JwtOptions> options,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            ITokenBlocklistService tokenBlocklist = null)
        {
            _userManager = userManager;
            _options = options;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _tokenBlocklist = tokenBlocklist;
        }

        // ─── LOGIN ──────────────────────────────────────────────────────────
        public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
                throw new UnauthorizedException("Invalid email or password.");

            var validPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!validPassword)
                throw new UnauthorizedException("Invalid email or password.");

            // Check if account is active
            if (!user.Active)
                throw new ForbiddenException("Account has been suspended by the administrator.");

            // Role validation — compare requested role with actual roles
            var roles = await _userManager.GetRolesAsync(user);
            var requestedRole = loginDto.Role?.Trim();
            if (!string.IsNullOrEmpty(requestedRole))
            {
                var hasRole = roles.Any(r => r.Equals(requestedRole, StringComparison.OrdinalIgnoreCase));
                if (!hasRole)
                    throw new ForbiddenException($"This account does not have the '{requestedRole}' role.");
            }

            var department = user.DepartmentId.HasValue
                ? await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value)
                : null;

            // Get current study year and semester for students
            int? currentStudyYearId = null;
            int? currentSemesterId  = null;
            try
            {
                var currentSY = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
                if (currentSY != null)
                {
                    currentStudyYearId = currentSY.Id;
                    var semesters = await _unitOfWork.Semesters.GetByStudyYearIdAsync(currentSY.Id);
                    var activeSem = semesters.FirstOrDefault(s => s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
                                   ?? semesters.OrderByDescending(s => s.StartDate).FirstOrDefault();
                    currentSemesterId = activeSem?.Id;
                }
            }
            catch { /* non-critical — student can still log in */ }

            // Role-based token expiry: admin = 8h, student/instructor = 24h
            var actualRole = roles.FirstOrDefault() ?? "Student";
            var token = await CreateTokenAsync(user, actualRole);

            return new UserResultDto
            {
                Id              = user.Id,
                DisplayName     = user.DisplayName,
                Token           = token,
                Email           = user.Email,
                Role            = actualRole,
                AcademicCode    = user.Academic_Code,
                UserName        = user.UserName,
                TotalCredits    = user.TotalCredits,
                AllowedCredits  = user.AllowedCredits ?? 21,
                TotalGPA        = user.TotalGPA,
                Specialization  = user.Specialization,
                Level           = user.Level,
                PhoneNumber     = user.PhoneNumber,
                DepartmentName  = department?.Name,
                Gender          = user.Gender,
                DepartmentId    = department?.Id,
                ProfilePicture  = user.ProfilePicture,
                Address         = user.Address,
                DateOfBirth     = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                EntryYear       = user.EntryYear,
                ThemeId         = user.ThemeId,
                CurrentStudyYearId = currentStudyYearId,
                CurrentSemesterId  = currentSemesterId,
                MustChangePassword = false,
            };
        }

        // ─── GET CURRENT USER (GET /api/auth/me) ───────────────────────────
        public async Task<UserResultDto> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"User not found.");

            if (!user.Active)
                throw new ForbiddenException("Account has been suspended.");

            var roles = await _userManager.GetRolesAsync(user);
            var department = user.DepartmentId.HasValue
                ? await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value)
                : null;

            return new UserResultDto
            {
                Id              = user.Id,
                DisplayName     = user.DisplayName,
                Email           = user.Email,
                Role            = roles.FirstOrDefault() ?? "Student",
                AcademicCode    = user.Academic_Code,
                UserName        = user.UserName,
                TotalCredits    = user.TotalCredits,
                AllowedCredits  = user.AllowedCredits,
                TotalGPA        = user.TotalGPA,
                Specialization  = user.Specialization,
                Level           = user.Level,
                PhoneNumber     = user.PhoneNumber,
                DepartmentName  = department?.Name,
                Gender          = user.Gender,
                DepartmentId    = department?.Id,
                ProfilePicture  = user.ProfilePicture,
                Address         = user.Address,
                DateOfBirth     = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                EntryYear       = user.EntryYear,
                ThemeId         = user.ThemeId,
            };
        }

        // ─── LOGOUT ─────────────────────────────────────────────────────────
        public async Task LogoutAsync(string token)
        {
            if (_tokenBlocklist != null && !string.IsNullOrWhiteSpace(token))
            {
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var jti = jwtToken.Id; // the "jti" claim
                    var expiry = jwtToken.ValidTo;

                    if (!string.IsNullOrWhiteSpace(jti))
                    {
                        await _tokenBlocklist.BlockTokenAsync(jti, expiry);
                    }
                }
                catch
                {
                    // Fallback: hash the token and block for 24h
                    var fallbackKey = Convert.ToBase64String(
                        System.Security.Cryptography.SHA256.HashData(
                            System.Text.Encoding.UTF8.GetBytes(token)));
                    await _tokenBlocklist.BlockTokenAsync(fallbackKey, DateTime.UtcNow.AddHours(24));
                }
            }
        }

        // ─── REGISTER ───────────────────────────────────────────────────────
        public async Task<UserResultDto> RegisterAsync(RegisterDto registerDto, string role = "Student")
        {
            var ChekInputValidation = new List<string>();

            if (await _userManager.Users.AnyAsync(u => u.Academic_Code == registerDto.Academic_Code))
                ChekInputValidation.Add("Academic Code already exists.");

            if (await _userManager.Users.AnyAsync(u => u.UserName == registerDto.UserName))
                ChekInputValidation.Add("UserName already exists.");

            if (await _userManager.Users.AnyAsync(u => u.Email == registerDto.Email))
                ChekInputValidation.Add("Email already exists.");


            if (ChekInputValidation.Any())
                throw new ValidationException(ChekInputValidation);


            var user = new User
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,   
                PhoneNumber = registerDto.PhoneNumber,
                Academic_Code = registerDto.Academic_Code,
                Gender = registerDto.Gender
            };



            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            
            // Add specified role
            if (await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(user, role);

          
            var roles = await _userManager.GetRolesAsync(user);

            var token = await CreateTokenAsync(user, roles.FirstOrDefault() ?? "Student");
            return new UserResultDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = token,
                AcademicCode = user.Academic_Code,
                Role = roles.FirstOrDefault(),
                UserName = user.UserName,
                TotalCredits = null,
                AllowedCredits = user.AllowedCredits,
                TotalGPA = user.TotalGPA,
                Specialization = user.Specialization,
                Level = user.Level,
                PhoneNumber = user.PhoneNumber,
                DepartmentName = null,
                DepartmentId = user.DepartmentId          
            };
        }

        public async Task<UserResultDto> RegisterStudentAsync(int departmentId, RegisterStudentDto registerStudentDto)
        {
            var ChekInputValidation = new List<string>();

            if (await _userManager.Users.AnyAsync(u => u.Academic_Code == registerStudentDto.Academic_Code))
                ChekInputValidation.Add("Academic Code already exists.");

            if (await _userManager.Users.AnyAsync(u => u.UserName == registerStudentDto.UserName))
                ChekInputValidation.Add("UserName already exists.");

            if (await _userManager.Users.AnyAsync(u => u.Email == registerStudentDto.Email))
                ChekInputValidation.Add("Email already exists.");


            if (ChekInputValidation.Any())
                throw new ValidationException(ChekInputValidation);

            // Determine starting level based on department
            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department == null)
                throw new NotFoundException($"There is no department with id '{departmentId}'.");

            var startingLevel = department.HasPreparatoryYear
                ? Levels.Preparatory_Year
                : Levels.First_Year;

            var allowedCredits = startingLevel == Levels.First_Year ? 21 : 0;

            var user = new User
            {
                DisplayName = registerStudentDto.DisplayName,
                Email = registerStudentDto.Email,
                UserName = registerStudentDto.UserName,
                PhoneNumber = registerStudentDto.PhoneNumber,
                Academic_Code = registerStudentDto.Academic_Code,
                Level = startingLevel,
                CurrentSemester = 1,
                TotalCredits = 0,
                AllowedCredits = allowedCredits,
                TotalGPA = 0,
                DepartmentId = departmentId,
                Gender = registerStudentDto.Gender,
                EntryYear = DateTime.UtcNow.Year.ToString()
            };



            var result = await _userManager.CreateAsync(user, registerStudentDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            
            // Add Student role
            if (await _roleManager.RoleExistsAsync("Student"))
                await _userManager.AddToRoleAsync(user, "Student");

          
            var roles = await _userManager.GetRolesAsync(user);

            var token = await CreateTokenAsync(user, "Student");
            return new UserResultDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = token,
                AcademicCode = user.Academic_Code,
                Role = roles.FirstOrDefault(),
                UserName = user.UserName,
                TotalCredits = user.TotalCredits, 
                AllowedCredits = user.AllowedCredits, 
                TotalGPA = user.TotalGPA, 
                Specialization = user.Specialization, 
                Level = user.Level, 
                PhoneNumber = user.PhoneNumber,
                DepartmentName = department.Name,
                ProfilePicture = user.ProfilePicture,
                Gender = user.Gender,
                EntryYear = user.EntryYear
            };
        }


        // ResetPasswordAsync
        public async Task<string> ResetPasswordAsync(string email, string newPassword) 
        { var user = await _userManager.FindByEmailAsync(email);

            if (user == null) throw new NotFoundException($"There is no user with email '{email}'.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user); 

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword); 

            return result.Succeeded ? "Password Updated Successfully" 
                : string.Join(" | ", result.Errors.Select(e => e.Description)); 
        
        }


        // UpdateRoleByEmailAsync
        public async Task<string> UpdateRoleByEmailAsync(string email, string newRole)
        {
            var user = await _userManager.FindByEmailAsync(email); if (user == null) throw new NotFoundException($"There is no user with email '{email}'.");

             if (!await _roleManager.RoleExistsAsync(newRole)) 
                throw new NotFoundException($"Role '{newRole}' does not exist.");
            // Remove old roles

            var oldRoles = await _userManager.GetRolesAsync(user); 
            await _userManager.RemoveFromRolesAsync(user, oldRoles); 

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (!result.Succeeded) 
                return "Failed to update role";
            
            return $"Role updated to {newRole} successfully"; }


        // ─── TOKEN CREATION ─────────────────────────────────────────────────
        private async Task<string> CreateTokenAsync(User user, string role = "Student")
        {
            var jwtOptions = _options.Value;

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Academic_Code ?? user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name , user.DisplayName ?? string.Empty),
                new Claim(ClaimTypes.Email , user.Email ?? string.Empty),
                new Claim("role", role.ToLower()),
                new Claim("email", user.Email ?? string.Empty),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var privateKey = await File.ReadAllTextAsync("Keys/private_key.pem");
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);
            var key = new RsaSecurityKey(rsa);

            var signingCreds = new SigningCredentials(key, SecurityAlgorithms.RsaSha512);

            // Role-based expiry: admin = 8h, student/instructor = 24h
            var expiryHours = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? 8 : 24;
            var expires = DateTime.UtcNow.AddHours(expiryHours);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: signingCreds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
