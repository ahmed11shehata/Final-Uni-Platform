using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Quiz;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class SubmitQuizCommandHandler
            : IRequestHandler<SubmitQuizCommand, Response<decimal>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubmitQuizCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<decimal>> Handle(
            SubmitQuizCommand request,
            CancellationToken cancellationToken)
        {
            var quiz = await _unitOfWork.Quizzes
                .GetQuizWithQuestionsAsync(request.Submission.QuizId);

            if (quiz == null)
                return Response<decimal>.ErrorResponse("Quiz not found");

            if (DateTime.UtcNow < quiz.StartTime)
                return Response<decimal>.ErrorResponse("Quiz not started yet");

            if (DateTime.UtcNow > quiz.EndTime)
                return Response<decimal>.ErrorResponse("Quiz ended");

            var attempted = await _unitOfWork.Quizzes
                .AttemptExists(quiz.Id, request.StudentId);

            if (attempted)
                return Response<decimal>.ErrorResponse("Quiz already submitted");

            decimal score = 0m;
            decimal gradePerQ = quiz.GradePerQuestion <= 0m ? 1m : quiz.GradePerQuestion;

            var answers = new List<StudentAnswer>();

            foreach (var answer in request.Submission.Answers)
            {
                var option = quiz.Questions
                    .SelectMany(q => q.Options)
                    .FirstOrDefault(o => o.Id == answer.SelectedOptionId);

                if (option == null)
                    continue;

                if (option.IsCorrect)
                    score += gradePerQ;

                answers.Add(new StudentAnswer
                {
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId
                });
            }

            var attempt = new StudentQuizAttempt
            {
                QuizId = quiz.Id,
                StudentId = request.StudentId,
                Score = score,
                SubmittedAt = DateTime.UtcNow,
                Answers = answers
            };

            await _unitOfWork.Quizzes.AddAttemptAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            return Response<decimal>.SuccessResponse(score);
        }
    }
}
