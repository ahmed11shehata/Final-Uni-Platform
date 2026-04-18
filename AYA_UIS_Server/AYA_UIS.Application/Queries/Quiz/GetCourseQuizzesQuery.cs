using MediatR;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Queries.Quiz
{
    public class GetCourseQuizzesQuery : IRequest<IEnumerable<FrontendQuizDto>>
    {
        public int CourseId { get; set; }
    }
}
