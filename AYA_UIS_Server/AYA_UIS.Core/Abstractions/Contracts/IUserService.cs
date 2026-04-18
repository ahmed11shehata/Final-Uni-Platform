using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Dtos.Auth_Module;
using Shared.Dtos.Info_Module.UserDtos;

namespace AYA_UIS.Application.Contracts
{
    public interface IUserService
    {
        Task<userProfileDetailsDto> GetUserProfileByAcademicCodeAsync(string academicCode);
        Task UpdateProfilePictureAsync(string userId, UpdateProfilePictureDto dto);
        Task UpdateStudentSpecializationAsync(string academicCode, UpdateStudentSpecializationDto dto);

        /// <summary>
        /// Gets the full user profile for the authenticated user (GET /api/user/profile).
        /// Returns the exact response shape the frontend expects.
        /// </summary>
        Task<UserProfileResponseDto> GetUserProfileAsync(string userId);

        /// <summary>
        /// Updates editable profile fields (PUT /api/user/profile).
        /// Returns the updated profile in the same shape.
        /// </summary>
        Task<UserProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);

        /// <summary>
        /// Uploads user avatar image (POST /api/user/avatar).
        /// </summary>
        Task<string> UploadAvatarAsync(string userId, UploadAvatarDto dto);

        /// <summary>
        /// Updates user theme preference (PUT /api/user/theme).
        /// </summary>
        Task UpdateThemeAsync(string userId, UpdateThemeDto dto);
    }
}