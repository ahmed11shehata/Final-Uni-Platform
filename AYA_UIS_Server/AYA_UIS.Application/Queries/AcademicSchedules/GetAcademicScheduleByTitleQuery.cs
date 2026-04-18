using MediatR;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;

namespace AYA_UIS.Application.Queries.AcademicSchedules
{
    public class GetAcademicScheduleByTitleQuery : IRequest<AcademicSchedulesDto>
    {
        public string ScheduleTitle { get; set; } = string.Empty;

        public GetAcademicScheduleByTitleQuery(string scheduleTitle)
        {
            ScheduleTitle = scheduleTitle;
        }
    }
}