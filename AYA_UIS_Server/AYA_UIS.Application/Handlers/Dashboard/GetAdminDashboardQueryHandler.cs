using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard
{
    public class GetAdminDashboardQueryHandler
        : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
    {
        private readonly IUnitOfWork      _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetAdminDashboardQueryHandler(
            IUnitOfWork      unitOfWork,
            UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<AdminDashboardDto> Handle(
            GetAdminDashboardQuery request,
            CancellationToken      ct)
        {
            var students    = await _userManager.GetUsersInRoleAsync("Student");
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            var courses     = await _unitOfWork.Courses.GetAllAsync();
            var regs        = await _unitOfWork.Registrations.GetAllAsync();
            var currentYear = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();

            // Read REAL registration status from DB
            var regSettings     = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            var registrationOpen = regSettings?.IsOpen ?? false;

            return new AdminDashboardDto
            {
                TotalStudents       = students.Count,
                TotalInstructors    = instructors.Count,
                TotalCourses        = courses.Count(),
                ActiveRegistrations = regs.Count(),
                RegistrationOpen    = registrationOpen,
                CurrentStudyYear    = currentYear is null ? null : new CurrentStudyYearDto
                {
                    Id        = currentYear.Id,
                    StartYear = currentYear.StartYear,
                    EndYear   = currentYear.EndYear
                },
                RecentActivity = new List<object>()
            };
        }
    }
}
