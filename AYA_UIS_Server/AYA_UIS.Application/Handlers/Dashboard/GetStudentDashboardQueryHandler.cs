using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard
{
    public class GetStudentDashboardQueryHandler
        : IRequestHandler<GetStudentDashboardQuery, StudentDashboardDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetStudentDashboardQueryHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<StudentDashboardDto> Handle(GetStudentDashboardQuery request, CancellationToken ct)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null) return new StudentDashboardDto();

            var depts = await _unitOfWork.Departments.GetAllAsync();
            var dept  = user.DepartmentId.HasValue
                ? depts.FirstOrDefault(d => d.Id == user.DepartmentId.Value) : null;

            var (standingLabel, standingColor) = GetStanding(user.TotalGPA);

            // Enrolled courses
            var regs = await _unitOfWork.Registrations.GetByUserAsync(user.Id);
            var activeCourseIds = regs.Where(r => !r.IsPassed).Select(r => r.CourseId).ToList();
            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var colors = new[]{"#6366f1","#3b82f6","#0ea5e9","#22c55e","#f59e0b","#ec4899","#8b5cf6","#e05c8a"};
            var enrolled = allCourses
                .Where(c => activeCourseIds.Contains(c.Id))
                .Select((c, i) => (object)new
                {
                    id         = c.Id,
                    code       = c.Code,
                    name       = c.Name,
                    credits    = c.Credits,
                    progress   = 0,
                    color      = colors[i % colors.Length],
                    icon       = "📚",
                    instructor = (string?)null
                }).ToList();

            // Upcoming quizzes
            // quizzes loaded per-course below
            var now      = DateTime.UtcNow;
            var quizList = new List<object>();
            foreach (var cId in activeCourseIds.Take(5))
            {
                var cqs    = await _unitOfWork.Quizzes.GetQuizzesByCourseId(cId);
                var course = allCourses.FirstOrDefault(x => x.Id == cId);
                foreach (var q in cqs.Where(q => q.StartTime >= now).Take(2))
                    quizList.Add(new { id = q.Id, type = "quiz", title = q.Title,
                        courseCode = course?.Code ?? "", course = course?.Name ?? "",
                        date = q.StartTime.ToString("yyyy-MM-dd"),
                        time = q.StartTime.ToString("hh:mm tt"), duration = (string?)null });
            }
            var upcomingQ = quizList.Take(4).ToList();

            return new StudentDashboardDto
            {
                Stats = new StudentStatsDto
                {
                    Gpa            = user.TotalGPA ?? 0,
                    Credits        = user.TotalCredits ?? 0,
                    AllowedCredits = user.AllowedCredits ?? 21,
                    Standing       = standingLabel,
                    StandingColor  = standingColor,
                    Year           = user.Level.HasValue ? (int)user.Level.Value : 1,
                    Semester       = 1,
                    Department     = dept?.Name ?? "Computer Science"
                },
                UpcomingEvents  = upcomingQ,
                EnrolledCourses = enrolled
            };
        }

        private static (string standing, string color) GetStanding(decimal? gpa) =>
            (gpa ?? 0) switch
            {
                >= 3.5m => ("Excellent",         "#22c55e"),
                >= 3.0m => ("Very Good",          "#4ade80"),
                >= 2.5m => ("Good",               "#86efac"),
                >= 2.0m => ("Pass",               "#f59e0b"),
                >= 1.5m => ("Academic Warning",   "#ef4444"),
                _       => ("Academic Probation", "#991b1b")
            };
    }
}
