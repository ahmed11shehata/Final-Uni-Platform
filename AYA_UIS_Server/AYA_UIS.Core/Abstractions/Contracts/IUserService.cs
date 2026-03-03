using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Dtos.Info_Module.UserDtos;

namespace AYA_UIS.Application.Contracts
{
    public interface IUserService
    {
        Task<userProfileDetailsDto> GetUserProfileByAcademicCodeAsync(string academicCode);
        Task UpdateProfilePictureAsync(string userId, UpdateProfilePictureDto dto);
        Task UpdateStudentSpecializationAsync(string academicCode, UpdateStudentSpecializationDto dto);
    }
}