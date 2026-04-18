using MediatR;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Queries.Dashboard
{
    public record GetStudentTranscriptQuery(string UserId) : IRequest<StudentTranscriptDto>;
}
