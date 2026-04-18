using AYA_UIS.Application.Queries.Assignments;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GetAssignmentsByCourseQueryHandler
        : IRequestHandler<GetAssignmentsByCourseQuery, IEnumerable<AssignmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignmentsByCourseQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AssignmentDto>> Handle(
            GetAssignmentsByCourseQuery request,
            CancellationToken           cancellationToken)
        {
            var assignments = await _unitOfWork.Assignments
                .GetAssignmentsByCourseIdAsync(request.CourseId);

            return assignments.Select(a => new AssignmentDto
            {
                Id             = a.Id,
                Title          = a.Title,
                Description    = a.Description,
                Points         = a.Points,
                Deadline       = a.Deadline,
                FileUrl        = a.FileUrl,
                CourseId       = a.CourseId,
                CourseName     = a.Course?.Name     ?? string.Empty,
                InstructorName = a.CreatedBy?.DisplayName ?? a.CreatedBy?.UserName ?? string.Empty
            });
        }
    }
}
