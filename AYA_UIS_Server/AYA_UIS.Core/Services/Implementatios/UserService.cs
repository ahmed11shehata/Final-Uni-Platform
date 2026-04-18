using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dtos.Auth_Module;
using Shared.Dtos.Info_Module.UserDtos;

namespace Services.Implementatios
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase)
        {
            "default", "ramadan", "space", "arctic", "pharaoh", "saladin", "fog"
        };

        public UserService(UserManager<User> userManager, ILogger<UserService> logger, ICloudinaryService cloudinaryService, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
        }

        // ─── GET PROFILE BY ACADEMIC CODE (existing) ────────────────────────
        public async Task<userProfileDetailsDto> GetUserProfileByAcademicCodeAsync(string academicCode)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Academic_Code == academicCode);

                if (user == null)
                    throw new NotFoundException($"User with academic code '{academicCode}' not found.");

                var department = user.DepartmentId.HasValue
                    ? await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value)
                    : null;

                var roles = await _userManager.GetRolesAsync(user);

                return new userProfileDetailsDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePicture = user.ProfilePicture,
                    AcademicCode = user.Academic_Code,
                    Level = user.Level,
                    TotalCredits = user.TotalCredits,
                    AllowedCredits = user.AllowedCredits,
                    TotalGPA = user.TotalGPA,
                    Specialization = user.Specialization,
                    DepartmentName = department.Name,
                    Role = roles.FirstOrDefault(),
                };
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new InternalServerErrorException($"An error occurred while retrieving the user profile for academic code '{academicCode}'.", ex);
            }
        }

        // ─── UPDATE PROFILE PICTURE (existing) ──────────────────────────────
        public async Task UpdateProfilePictureAsync(string userId, UpdateProfilePictureDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new NotFoundException($"User with ID '{userId}' not found.");

                user.ProfilePicture = await _cloudinaryService.UploadUserProfilePictureAsync(dto.ProfilePicture, userId);

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    throw new ValidationException(errors);
                }
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new InternalServerErrorException($"An error occurred while updating the profile picture for user ID '{userId}'.", ex);
            }
        }

        // ─── UPDATE STUDENT SPECIALIZATION (existing) ───────────────────────
        public async Task UpdateStudentSpecializationAsync(string academicCode, UpdateStudentSpecializationDto dto)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Academic_Code == academicCode);

                if (user == null)
                    throw new NotFoundException($"Student with academic code '{academicCode}' not found.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Student"))
                {
                    var errors = roles.Select(r => $"User does not have the required role: {r}").ToList();
                    throw new ValidationException(errors);
                }

                user.Specialization = dto.Specialization;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    throw new ValidationException(errors);
                }
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new InternalServerErrorException($"An error occurred while updating the specialization for student with academic code '{academicCode}'.", ex);
            }
        }

        // ─── GET USER PROFILE (GET /api/user/profile) ───────────────────────
        public async Task<UserProfileResponseDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = (roles.FirstOrDefault() ?? "student").ToLower();
            var department = user.DepartmentId.HasValue
                ? await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value)
                : null;

            var profileUser = new UserProfileUserDto
            {
                Id           = user.Id,
                AcademicCode = user.Academic_Code,
                Name         = user.DisplayName ?? string.Empty,
                Email      = user.Email ?? string.Empty,
                Role       = role,
                Department = department?.Name,
                Phone      = user.PhoneNumber,
                Address    = user.Address,
                Dob        = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                Gender     = user.Gender.ToString().ToLower(),
                Avatar     = string.IsNullOrEmpty(user.ProfilePicture) ? null : user.ProfilePicture,
                ThemeId    = user.ThemeId ?? "default",
            };

            return new UserProfileResponseDto
            {
                Success = true,
                Data = new UserProfileDataDto { User = profileUser }
            };
        }

        // ─── UPDATE PROFILE (PUT /api/user/profile) ─────────────────────────
        public async Task<UserProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            // Reject read-only fields
            if (!string.IsNullOrWhiteSpace(dto.Email))
                throw new BadRequestException("Field 'email' is read-only and cannot be updated.");
            if (!string.IsNullOrWhiteSpace(dto.Role))
                throw new BadRequestException("Field 'role' is read-only and cannot be updated.");
            if (!string.IsNullOrWhiteSpace(dto.Department))
                throw new BadRequestException("Field 'department' is read-only and cannot be updated.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            // Update only provided fields — DisplayName is read-only after creation
            if (!string.IsNullOrWhiteSpace(dto.Name))
                throw new BadRequestException("Field 'name' is read-only and cannot be updated.");

            if (dto.Phone != null)
                user.PhoneNumber = dto.Phone;

            if (dto.Address != null)
                user.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.Dob))
            {
                if (DateTime.TryParseExact(dto.Dob, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var dob))
                    user.DateOfBirth = dob;
                else
                    throw new BadRequestException("Invalid date of birth format. Use YYYY-MM-DD.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Gender))
            {
                if (Enum.TryParse<Gender>(dto.Gender, true, out var gender))
                    user.Gender = gender;
                else
                    throw new BadRequestException("Gender must be 'male' or 'female'.");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            return await GetUserProfileAsync(userId);
        }

        // ─── UPLOAD AVATAR (POST /api/user/avatar) ─────────────────────────
        public async Task<string> UploadAvatarAsync(string userId, UploadAvatarDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (dto.File == null || dto.File.Length == 0)
                throw new BadRequestException("File is required.");

            // Validate file size — max 5MB
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (dto.File.Length > maxFileSize)
                throw new BadRequestException("Avatar file size must not exceed 5MB.");

            // Validate file is an image (jpg, jpeg, png, webp only)
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = System.IO.Path.GetExtension(dto.File.FileName)?.ToLower();
            if (!allowedContentTypes.Contains(dto.File.ContentType.ToLower()) ||
                (!string.IsNullOrEmpty(ext) && !allowedExtensions.Contains(ext)))
                throw new BadRequestException("Only image files are allowed (jpg, jpeg, png, webp).");

            // Upload via Cloudinary
            var avatarUrl = await _cloudinaryService.UploadUserProfilePictureAsync(dto.File, userId);

            // Save to user
            user.ProfilePicture = avatarUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            return avatarUrl;
        }

        // ─── UPDATE THEME (PUT /api/user/theme) ─────────────────────────────
        public async Task UpdateThemeAsync(string userId, UpdateThemeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ThemeId) || !AllowedThemes.Contains(dto.ThemeId))
                throw new BadRequestException($"Invalid themeId '{dto.ThemeId}'. Allowed values: {string.Join(", ", AllowedThemes)}.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            user.ThemeId = dto.ThemeId.ToLower();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }
        }
    }
}