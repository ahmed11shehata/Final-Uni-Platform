using MediatR;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Queries.Quiz
{
    public class GetQuizQuery : IRequest<FrontendQuizDto?>
    {
        public int QuizId { get; set; }
    }
}
