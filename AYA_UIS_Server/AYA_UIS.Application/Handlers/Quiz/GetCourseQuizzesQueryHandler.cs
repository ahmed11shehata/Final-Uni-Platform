using AYA_UIS.Application.Queries.Quiz;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class GetCourseQuizzesQueryHandler
        : IRequestHandler<GetCourseQuizzesQuery, IEnumerable<FrontendQuizDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseQuizzesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<FrontendQuizDto>> Handle(
            GetCourseQuizzesQuery request,
            CancellationToken cancellationToken)
        {
            var quizzes = await _unitOfWork.Quizzes
                .GetQuizzesByCourseId(request.CourseId);

            return quizzes.Select(q => new FrontendQuizDto
            {
                Id            = q.Id,
                Title         = q.Title,
                CourseId      = q.CourseId,
                CourseCode    = q.Course?.Code ?? string.Empty,
                StartTime     = q.StartTime,
                EndTime       = q.EndTime,
                Duration      = (int)Math.Round((q.EndTime - q.StartTime).TotalMinutes),
                QuestionCount = q.Questions?.Count ?? 0,
                Questions     = null  // not loaded in list view
            });
        }
    }
}
