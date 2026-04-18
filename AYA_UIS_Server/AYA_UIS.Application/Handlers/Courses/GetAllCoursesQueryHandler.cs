using AYA_UIS.Application.Queries.Courses;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.Courses
{
    public class GetAllCoursesQueryHandler
        : IRequestHandler<GetAllCoursesQuery, IEnumerable<FrontendCourseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllCoursesQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<FrontendCourseDto>> Handle(
            GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var courses = await _unitOfWork.Courses.GetAllAsync();
            var colors  = new[] { "#6366f1","#8b5cf6","#0ea5e9","#f59e0b","#ef4444","#14b8a6","#e05c8a","#22c55e" };

            return courses.Select((c, i) => new FrontendCourseDto
            {
                Id          = c.Id,
                Code        = c.Code,
                Name        = c.Name,
                Credits     = c.Credits,
                Year        = ExtractYear(c.Code),
                Semester    = 1,
                Type        = "mandatory",
                Status      = c.Status.ToString(),
                Dept        = c.Department?.Name ?? string.Empty,
                Prereqs     = Array.Empty<string>(),
                Color       = colors[i % colors.Length],
                Instructor  = null,
                Description = null,
                RegStatus   = "available",
                Grade       = null,
                IsPassed    = false
            });
        }

        /// <summary>Extract academic year from course code. CS1011, CS2012, H1051</summary>
        internal static int ExtractYear(string code)
        {
            if (string.IsNullOrEmpty(code)) return 1;
            // First digit in the code = year number
            foreach (char ch in code)
                if (char.IsDigit(ch) && ch >= '1' && ch <= '4')
                    return ch - '0';
            return 1;
        }
    }
}
