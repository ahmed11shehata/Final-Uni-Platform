using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Queries.Quiz;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.QuizDto;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class GetQuizQueryHandler
     : IRequestHandler<GetQuizQuery,
         Response<IEnumerable<QuizQuestionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetQuizQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<QuizQuestionDto>>> Handle(
            GetQuizQuery request,
            CancellationToken cancellationToken)
        {
            var quiz = await _unitOfWork.Quizzes
                .GetQuizWithQuestionsAsync(request.QuizId);

            if (quiz == null)
                return Response<IEnumerable<QuizQuestionDto>>
                    .ErrorResponse("Quiz not found");

            var questions = quiz.Questions.Select(q =>
                new QuizQuestionDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options.Select(o =>
                        new QuizOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text
                        }).ToList()
                });

            return Response<IEnumerable<QuizQuestionDto>>
                .SuccessResponse(questions);
        }
    }
}
