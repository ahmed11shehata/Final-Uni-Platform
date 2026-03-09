using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.QuizDto;
using Shared.Respones;

namespace AYA_UIS.Application.Queries.Quiz
{
    public class GetCourseQuizzesQuery
    : IRequest<Response<IEnumerable<QuizDto>>>
    {
        public int CourseId { get; set; }
    }
}
