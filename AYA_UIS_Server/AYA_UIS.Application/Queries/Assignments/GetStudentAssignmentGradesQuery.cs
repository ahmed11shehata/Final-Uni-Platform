using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Queries.Assignments
{
    public class GetStudentAssignmentGradesQuery
        : IRequest<IEnumerable<StudentAssignmentGradeDto>>
    {
        public int    CourseId  { get; set; }
        public string StudentId { get; set; } = string.Empty;
    }
}
