using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Queries.Assignments
{
    public class GetAssignmentSubmissionsQuery : IRequest<IEnumerable<AssignmentSubmissionDto>>
    {
        public int AssignmentId { get; set; }
    }
}
