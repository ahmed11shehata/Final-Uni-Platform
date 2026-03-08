using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Queries.Assignments;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GetStudentAssignmentGradesQueryHandler
: IRequestHandler<GetStudentAssignmentGradesQuery,
    Response<IEnumerable<StudentAssignmentGradeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentAssignmentGradesQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<StudentAssignmentGradeDto>>> Handle(
            GetStudentAssignmentGradesQuery request,
            CancellationToken cancellationToken)
        {
            var assignments = await _unitOfWork.Assignments
                 .GetAssignmentsWithSubmissions(request.CourseId);

            var result = assignments.Select(a =>
            {
                var submission = a.Submissions?
                    .FirstOrDefault(s => s.StudentId == request.StudentId);

                return new StudentAssignmentGradeDto
                {
                    AssignmentId = a.Id,
                    AssignmentTitle = a.Title,
                    Points = a.Points,
                    Deadline = a.Deadline,
                    FileUrl = a.FileUrl,
                    Grade = submission?.Grade,
                    Feedback = submission?.Feedback,
                    SubmittedAt = submission?.SubmittedAt
                };
            }).ToList();

            return Response<IEnumerable<StudentAssignmentGradeDto>>
                .SuccessResponse(result);
        }
    }
}
