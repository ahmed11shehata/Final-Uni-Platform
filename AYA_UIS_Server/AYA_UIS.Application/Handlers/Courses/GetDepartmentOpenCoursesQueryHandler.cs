using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.Courses
{
    public class GetDepartmentOpenCoursesQueryHandler
        : IRequestHandler<GetDepartmentOpenCoursesQuery, IEnumerable<FrontendCourseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetDepartmentOpenCoursesQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<FrontendCourseDto>> Handle(
            GetDepartmentOpenCoursesQuery request, CancellationToken cancellationToken)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId);
            if (department == null)
                throw new NotFoundException("No department found");

            var courses = await _unitOfWork.Courses.GetOpenCoursesAsync();
            var colors  = new[] { "#6366f1","#8b5cf6","#0ea5e9","#f59e0b","#ef4444","#14b8a6","#e05c8a","#22c55e" };

            return courses.Select((c, i) => new FrontendCourseDto
            {
                Id          = c.Id,
                Code        = c.Code,
                Name        = c.Name,
                Credits     = c.Credits,
                Year        = GetAllCoursesQueryHandler.ExtractYear(c.Code),
                Semester    = 1,
                Type        = "mandatory",
                Status      = c.Status.ToString(),
                Dept        = department.Name,
                Prereqs     = Array.Empty<string>(),
                Color       = colors[i % colors.Length],
                Instructor  = null,
                Description = null,
                RegStatus   = "available",
                Grade       = null,
                IsPassed    = false
            });
        }
    }
}
