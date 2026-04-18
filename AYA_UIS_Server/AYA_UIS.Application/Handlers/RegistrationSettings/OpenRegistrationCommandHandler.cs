using AYA_UIS.Application.Commands.RegistrationSettings;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

// Use fully-qualified name to avoid namespace clash with the folder name
using RegSettingsEntity = AYA_UIS.Core.Domain.Entities.Models.RegistrationSettings;

namespace AYA_UIS.Application.Handlers.RegistrationSettings
{
    public class OpenRegistrationCommandHandler
        : IRequestHandler<OpenRegistrationCommand, RegistrationStatusDto>
    {
        private readonly IUnitOfWork _uow;

        public OpenRegistrationCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<RegistrationStatusDto> Handle(
            OpenRegistrationCommand request,
            CancellationToken       ct)
        {
            var dto = request.Dto;

            if (string.IsNullOrWhiteSpace(dto.Semester))
                throw new ArgumentException("Semester is required.");

            if (dto.Deadline < DateTime.UtcNow)
                throw new ArgumentException("Deadline must be in the future.");

            var openYearsStr      = string.Join(",", dto.OpenYears ?? new List<int>());
            var enabledCoursesStr = string.Join(",", dto.EnabledCourses ?? new List<string>());

            var existing = await _uow.RegistrationSettings.GetCurrentAsync();

            if (existing is not null)
            {
                existing.IsOpen         = true;
                existing.Semester       = dto.Semester.Trim();
                existing.AcademicYear   = dto.AcademicYear?.Trim() ?? string.Empty;
                existing.StartDate      = dto.StartDate;
                existing.Deadline       = dto.Deadline;
                existing.OpenYears      = openYearsStr;
                existing.EnabledCourses = enabledCoursesStr;
                existing.OpenedAt       = DateTime.UtcNow;
                await _uow.RegistrationSettings.UpdateAsync(existing);
            }
            else
            {
                await _uow.RegistrationSettings.AddAsync(new RegSettingsEntity
                {
                    IsOpen         = true,
                    Semester       = dto.Semester.Trim(),
                    AcademicYear   = dto.AcademicYear?.Trim() ?? string.Empty,
                    StartDate      = dto.StartDate,
                    Deadline       = dto.Deadline,
                    OpenYears      = openYearsStr,
                    EnabledCourses = enabledCoursesStr,
                    OpenedAt       = DateTime.UtcNow
                });
            }

            await _uow.SaveChangesAsync();

            var daysLeft = (int)Math.Ceiling((dto.Deadline - DateTime.UtcNow).TotalDays);

            return new RegistrationStatusDto
            {
                IsOpen         = true,
                Semester       = dto.Semester,
                AcademicYear   = dto.AcademicYear ?? string.Empty,
                StartDate      = dto.StartDate,
                Deadline       = dto.Deadline,
                OpenYears      = dto.OpenYears ?? new List<int>(),
                EnabledCourses = dto.EnabledCourses ?? new List<string>(),
                DaysLeft       = Math.Max(0, daysLeft)
            };
        }
    }
}
