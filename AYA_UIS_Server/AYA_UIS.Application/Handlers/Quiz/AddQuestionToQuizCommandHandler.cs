using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Quiz;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class AddQuestionToQuizCommandHandler
            : IRequestHandler<AddQuestionToQuizCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddQuestionToQuizCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> Handle(
            AddQuestionToQuizCommand request,
            CancellationToken cancellationToken)
        {
            var quiz = await _unitOfWork.Quizzes
                .GetQuizAsync(request.QuizId);

            if (quiz == null)
                return Response<int>.ErrorResponse("Quiz not found");

            var question = new QuizQuestion
            {
                QuestionText = request.Question.QuestionText,
                QuizId = request.QuizId,
                Type = request.Question.Type
            };

            foreach (var option in request.Question.Options)
            {
                question.Options.Add(new QuizOption
                {
                    Text = option.Text,
                    IsCorrect = option.IsCorrect
                });
            }

            await _unitOfWork.Quizzes.AddQuestionAsync(question);

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.SuccessResponse(question.Id);
        }
    }
}
