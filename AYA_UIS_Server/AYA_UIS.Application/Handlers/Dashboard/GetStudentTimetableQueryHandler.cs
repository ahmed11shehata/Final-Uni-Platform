using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard
{
    public class GetStudentTimetableQueryHandler
        : IRequestHandler<GetStudentTimetableQuery, IEnumerable<TimetableEventDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetStudentTimetableQueryHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<TimetableEventDto>> Handle(
            GetStudentTimetableQuery request, CancellationToken ct)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null) return Enumerable.Empty<TimetableEventDto>();

            var regs   = await _unitOfWork.Registrations.GetByUserAsync(request.UserId);
            var cIds   = regs.Where(r => !r.IsPassed).Select(r => r.CourseId).ToList();
            var courses = await _unitOfWork.Courses.GetAllAsync();
            var cMap   = courses.ToDictionary(c => c.Id);

            var events = new List<TimetableEventDto>();
            var now    = DateTime.UtcNow;
            int id     = 1;

            // Quiz events
            foreach (var cId in cIds.Take(6))
            {
                var quizzes = await _unitOfWork.Quizzes.GetQuizzesByCourseId(cId);
                var course  = cMap.ContainsKey(cId) ? cMap[cId] : null;
                foreach (var q in quizzes.Where(q => q.StartTime >= now.AddDays(-7)))
                {
                    events.Add(new TimetableEventDto
                    {
                        Id         = id++,
                        Type       = "quiz",
                        Title      = q.Title,
                        CourseCode = course?.Code ?? "",
                        Course     = course?.Name ?? "",
                        Date       = q.StartTime.ToString("yyyy-MM-dd"),
                        Time       = q.StartTime.ToString("hh:mm tt"),
                        Duration   = $"{(int)(q.EndTime - q.StartTime).TotalMinutes} min"
                    });
                }

                // Assignment events
                var assignments = await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(cId);
                foreach (var a in assignments.Where(a => a.Deadline >= now.AddDays(-7)))
                {
                    events.Add(new TimetableEventDto
                    {
                        Id         = id++,
                        Type       = "assignment",
                        Title      = a.Title,
                        CourseCode = course?.Code ?? "",
                        Course     = course?.Name ?? "",
                        Date       = a.Deadline.ToString("yyyy-MM-dd"),
                        Time       = a.Deadline.ToString("hh:mm tt"),
                        Duration   = null
                    });
                }

                // Lecture uploads
                var uploads = await _unitOfWork.CourseUploads.GetByCourseIdAsync(cId);
                foreach (var u in uploads.Where(u => u.UploadedAt >= now.AddDays(-14)).Take(3))
                {
                    events.Add(new TimetableEventDto
                    {
                        Id         = id++,
                        Type       = "lecture",
                        Title      = u.Title,
                        CourseCode = course?.Code ?? "",
                        Course     = course?.Name ?? "",
                        Date       = u.UploadedAt.ToString("yyyy-MM-dd"),
                        Time       = "Uploaded",
                        Duration   = null
                    });
                }
            }

            return events.OrderBy(e => e.Date).ToList();
        }
    }
}
