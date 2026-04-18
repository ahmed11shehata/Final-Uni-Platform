using MediatR;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Queries.Dashboard
{
    public record GetStudentTimetableQuery(string UserId) : IRequest<IEnumerable<TimetableEventDto>>;
}
