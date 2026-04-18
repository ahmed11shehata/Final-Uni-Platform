using AYA_UIS.Application.Queries.Assignments;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GetAssignmentSubmissionsQueryHandler
        : IRequestHandler<GetAssignmentSubmissionsQuery, IEnumerable<AssignmentSubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignmentSubmissionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AssignmentSubmissionDto>> Handle(
            GetAssignmentSubmissionsQuery request,
            CancellationToken             cancellationToken)
        {
            var submissions = await _unitOfWork.Assignments
                .GetSubmissions(request.AssignmentId);

            return submissions.Select(s => new AssignmentSubmissionDto
            {
                Id          = s.Id,
                StudentId   = s.StudentId,
                StudentName = s.Student?.DisplayName ?? s.Student?.UserName ?? "Unknown",
                FileUrl     = s.FileUrl,
                SubmittedAt = s.SubmittedAt,
                Grade       = s.Grade,
                Feedback    = s.Feedback ?? string.Empty
            });
        }
    }
}
