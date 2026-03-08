using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace AYA_UIS.Application.Queries.Assignments
{
    public class GetStudentAssignmentGradesQuery
: IRequest<Response<IEnumerable<StudentAssignmentGradeDto>>>
    {
        public int CourseId { get; set; }

        public string StudentId { get; set; }
    }
}
