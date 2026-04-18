using AYA_UIS.Application.Queries.RegistrationSettings;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace AYA_UIS.Application.Handlers.RegistrationSettings
{
    public class GetRegistrationStatusQueryHandler
        : IRequestHandler<GetRegistrationStatusQuery, RegistrationStatusDto>
    {
        private readonly IUnitOfWork _uow;

        public GetRegistrationStatusQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<RegistrationStatusDto> Handle(
            GetRegistrationStatusQuery request,
            CancellationToken          ct)
        {
            var s = await _uow.RegistrationSettings.GetCurrentAsync();

            if (s is null)
                return new RegistrationStatusDto { IsOpen = false };

            var daysLeft = s.Deadline.HasValue
                ? Math.Max(0, (int)Math.Ceiling((s.Deadline.Value - DateTime.UtcNow).TotalDays))
                : 0;

            var openYears = string.IsNullOrWhiteSpace(s.OpenYears)
                ? new List<int>()
                : s.OpenYears
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => int.TryParse(x, out var n) ? n : 0)
                    .Where(n => n > 0)
                    .ToList();

            var enabledCourses = string.IsNullOrWhiteSpace(s.EnabledCourses)
                ? new List<string>()
                : s.EnabledCourses
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

            return new RegistrationStatusDto
            {
                IsOpen         = s.IsOpen,
                Semester       = s.Semester,
                AcademicYear   = s.AcademicYear,
                StartDate      = s.StartDate,
                Deadline       = s.Deadline,
                OpenYears      = openYears,
                EnabledCourses = enabledCourses,
                DaysLeft       = daysLeft
            };
        }
    }
}
