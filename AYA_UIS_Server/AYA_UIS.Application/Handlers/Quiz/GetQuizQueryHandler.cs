using AYA_UIS.Application.Queries.Quiz;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class GetQuizQueryHandler
        : IRequestHandler<GetQuizQuery, FrontendQuizDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetQuizQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<FrontendQuizDto?> Handle(
            GetQuizQuery request,
            CancellationToken cancellationToken)
        {
            var quiz = await _unitOfWork.Quizzes
                .GetQuizWithQuestionsAsync(request.QuizId);

            if (quiz == null) return null;

            return new FrontendQuizDto
            {
                Id            = quiz.Id,
                Title         = quiz.Title,
                CourseId      = quiz.CourseId,
                CourseCode    = quiz.Course?.Code ?? string.Empty,
                StartTime     = quiz.StartTime,
                EndTime       = quiz.EndTime,
                Duration      = (int)Math.Round((quiz.EndTime - quiz.StartTime).TotalMinutes),
                QuestionCount = quiz.Questions?.Count ?? 0,
                Questions     = quiz.Questions?.Select(q => new FrontendQuizQuestionDto
                {
                    Id   = q.Id,
                    Text = q.QuestionText,
                    Type = "MultipleChoice",
                    Options = q.Options?.Select(o => new FrontendQuizOptionDto
                    {
                        Id   = o.Id,
                        Text = o.Text
                    }).ToList() ?? new()
                }).ToList()
            };
        }
    }
}
