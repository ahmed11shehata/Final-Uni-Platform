using AYA_UIS.Application.Queries.Quiz;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class GetQuizAttemptsQueryHandler
        : IRequestHandler<GetQuizAttemptsQuery, IEnumerable<QuizAttemptDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetQuizAttemptsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<QuizAttemptDto>> Handle(
            GetQuizAttemptsQuery request,
            CancellationToken    cancellationToken)
        {
            var attempts = await _unitOfWork.Quizzes.GetAttemptsByQuizIdAsync(request.QuizId);

            return attempts.Select(a => new QuizAttemptDto
            {
                StudentId   = a.StudentId,
                StudentName = a.Student?.DisplayName ?? a.Student?.UserName ?? "Unknown",
                Score       = a.Score,
                SubmittedAt = a.SubmittedAt
            });
        }
    }
}
