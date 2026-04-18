using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Queries.Assignments
{
    public class GetAssignmentsByCourseQuery : IRequest<IEnumerable<AssignmentDto>>
    {
        public int CourseId { get; set; }
    }
}
