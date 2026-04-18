using Shared.Dtos.Student_Module;

namespace AYA_UIS.Application.Contracts
{
    public interface IStudentRegistrationService
    {
        Task<RegistrationStatusDto> GetRegistrationStatusAsync(string userId);
        Task<RegistrationCoursesDto> GetAvailableCoursesAsync(string userId);
        Task<RegistrationResponseDto> RegisterCourseAsync(string userId, string courseCode);
        Task DropCourseAsync(string userId, string courseCode);
        Task<List<StudentCourseDto>> GetEnrolledCoursesAsync(string userId);
        Task<FullCourseDetailDto> GetCourseDetailAsync(string userId, string courseId);
    }
}
