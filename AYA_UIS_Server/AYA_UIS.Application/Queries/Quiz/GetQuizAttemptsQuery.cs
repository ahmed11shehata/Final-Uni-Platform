using MediatR;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Queries.Quiz
{
    public record GetQuizAttemptsQuery(int QuizId) : IRequest<IEnumerable<QuizAttemptDto>>;
}
