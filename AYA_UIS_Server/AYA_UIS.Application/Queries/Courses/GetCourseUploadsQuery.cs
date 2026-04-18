using MediatR;
using Shared.Dtos.Info_Module.CourseUploadDtos;

namespace AYA_UIS.Application.Queries.Courses
{
    public record GetCourseUploadsQuery(int CourseId)
        : IRequest<IEnumerable<CourseUploadDto>>;
}
