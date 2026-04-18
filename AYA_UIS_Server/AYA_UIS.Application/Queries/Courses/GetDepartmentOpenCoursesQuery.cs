using MediatR;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Queries.Courses
{
    public record GetDepartmentOpenCoursesQuery(int DepartmentId)
        : IRequest<IEnumerable<FrontendCourseDto>>;
}
