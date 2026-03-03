using Microsoft.AspNetCore.Http;

namespace AYA_UIS.Application.Contracts
{
    public interface ICloudinaryService
    {
        Task<string> UploadUserProfilePictureAsync(IFormFile file, string userId, CancellationToken cancellationToken = default);
        Task<string> UploadAcademicScheduleAsync(IFormFile file, string scheduleId, CancellationToken cancellationToken = default);
        Task<string> UploadCourseFileAsync(IFormFile file, string fileId, string courseName, CancellationToken cancellationToken = default);
        Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
    }
}
