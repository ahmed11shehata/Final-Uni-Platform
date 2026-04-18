using MediatR;
using Shared.Dtos.Info_Module.AdminCourseLockDtos;

namespace AYA_UIS.Application.Commands.AdminCourseLock
{
    public record UnlockCourseCommand(string AcademicCode, int CourseId) : IRequest<AdminCourseLockResultDto>;
}
