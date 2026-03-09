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
    public class GetCourseQuizzesQueryHandler
    : IRequestHandler<GetCourseQuizzesQuery,
        Response<IEnumerable<QuizDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCourseQuizzesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<QuizDto>>> Handle(
            GetCourseQuizzesQuery request,
            CancellationToken cancellationToken)
        {
            var quizzes = await _unitOfWork.Quizzes
                .GetQuizzesByCourseId(request.CourseId);

            var result = quizzes.Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                StartTime = q.StartTime,
                EndTime = q.EndTime
            });

            return Response<IEnumerable<QuizDto>>
                .SuccessResponse(result);
        }
    }
}
