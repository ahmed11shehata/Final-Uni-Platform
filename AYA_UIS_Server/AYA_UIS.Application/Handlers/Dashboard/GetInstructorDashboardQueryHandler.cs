using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard
{
    public class GetInstructorDashboardQueryHandler
        : IRequestHandler<GetInstructorDashboardQuery, InstructorDashboardDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetInstructorDashboardQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<InstructorDashboardDto> Handle(
            GetInstructorDashboardQuery request, CancellationToken ct)
        {
            // Get courses this instructor uploaded materials for
            var uploads    = await _unitOfWork.CourseUploads.GetByUserIdAsync(request.UserId);
            var allCourses = await _unitOfWork.Courses.GetAllAsync();

            var courseIds  = uploads.Select(u => u.CourseId).Distinct().ToList();
            var myCourses  = allCourses.Where(c => courseIds.Contains(c.Id)).ToList();

            var colors = new[] { "#f59e0b","#e05c8a","#818cf8","#3b82f6","#22c55e","#ef4444" };

            // Grade summary per course
            var gradeSummary = new Dictionary<string, object>();
            foreach (var (course, i) in myCourses.Select((c, i) => (c, i)))
            {
                var assignsWithSubs = await _unitOfWork.Assignments
                    .GetAssignmentsWithSubmissions(course.Id);
                var subs    = assignsWithSubs
                    .SelectMany(a => a.Submissions ?? new List<AYA_UIS.Core.Domain.Entities.Models.AssignmentSubmission>())
                    .ToList();
                var pending  = subs.Count(s => s.Grade == null);
                var approved = subs.Count(s => s.Grade != null && s.Grade > 0);
                var avg      = subs.Where(s => s.Grade != null)
                                   .Select(s => (decimal)s.Grade!.Value)
                                   .DefaultIfEmpty(0m).Average();

                gradeSummary[course.Code.ToLower().Replace(" ", "")] =
                    new { pending, approved, rejected = 0, avg = avg.ToString("F1") };
            }

            // Upcoming quizzes
            var now      = DateTime.UtcNow;
            var upcoming = new List<object>();
            foreach (var course in myCourses.Take(3))
            {
                var quizzes = await _unitOfWork.Quizzes.GetQuizzesByCourseId(course.Id);
                foreach (var q in quizzes.Where(q => q.StartTime >= now).Take(2))
                    upcoming.Add(new
                    {
                        id    = q.Id,
                        title = q.Title,
                        date  = q.StartTime.ToString("MMM d"),
                        time  = q.StartTime.ToString("hh:mm tt"),
                        room  = "Online",
                        color = "#818cf8",
                        icon  = "✏️"
                    });
            }

            return new InstructorDashboardDto
            {
                Courses = myCourses.Select((c, i) => new InstructorCourseDto
                {
                    Id       = c.Code.ToLower().Replace(" ", ""),
                    Code     = c.Code,
                    Name     = c.Name,
                    Color    = colors[i % colors.Length],
                    Icon     = "📚",
                    Students = 0,
                    Progress = 0
                }).ToList(),
                GradeSummary   = gradeSummary,
                RecentActivity = new List<object>(),
                Upcoming       = upcoming.Take(5).ToList()
            };
        }
    }
}
