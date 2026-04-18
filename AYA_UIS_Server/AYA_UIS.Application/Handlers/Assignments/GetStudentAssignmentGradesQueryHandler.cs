using AYA_UIS.Application.Queries.Assignments;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GetStudentAssignmentGradesQueryHandler
        : IRequestHandler<GetStudentAssignmentGradesQuery, IEnumerable<StudentAssignmentGradeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentAssignmentGradesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<StudentAssignmentGradeDto>> Handle(
            GetStudentAssignmentGradesQuery request,
            CancellationToken               cancellationToken)
        {
            var assignments = await _unitOfWork.Assignments
                .GetAssignmentsByCourseIdAsync(request.CourseId);

            var result = new List<StudentAssignmentGradeDto>();

            foreach (var a in assignments)
            {
                var subs = await _unitOfWork.Assignments.GetSubmissions(a.Id);
                var mySub = subs.FirstOrDefault(s => s.StudentId == request.StudentId);

                result.Add(new StudentAssignmentGradeDto
                {
                    AssignmentId    = a.Id,
                    AssignmentTitle = a.Title,
                    Points          = a.Points,
                    Deadline        = a.Deadline,
                    SubmittedAt     = mySub?.SubmittedAt,
                    Grade           = mySub?.Grade,
                    Feedback        = mySub?.Feedback,
                    FileUrl         = mySub?.FileUrl
                });
            }

            return result;
        }
    }
}
