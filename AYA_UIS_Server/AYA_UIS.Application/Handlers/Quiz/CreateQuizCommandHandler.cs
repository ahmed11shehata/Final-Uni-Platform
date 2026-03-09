using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Quiz;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Quiz
{
    public class CreateQuizCommandHandler
            : IRequestHandler<CreateQuizCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateQuizCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> Handle(
            CreateQuizCommand request,
            CancellationToken cancellationToken)
        {
            var quiz = new Core.Domain.Entities.Models.Quiz
            {
                Title = request.QuizDto.Title,
                StartTime = request.QuizDto.StartTime,
                EndTime = request.QuizDto.EndTime,
                CourseId = request.QuizDto.CourseId
            };

            await _unitOfWork.Quizzes.AddQuizAsync(quiz);

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.SuccessResponse(quiz.Id);
        }
    }
}
