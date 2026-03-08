using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Assignment;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GradeSubmissionCommandHandler
 : IRequestHandler<GradeSubmissionCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeSubmissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> Handle(
            GradeSubmissionCommand request,
            CancellationToken cancellationToken)
        {
            var submission = await _unitOfWork.Assignments
                .GetSubmissionByIdAsync(request.SubmissionId);

            if (submission == null)
                return Response<int>.ErrorResponse("Submission not found");

            submission.Grade = request.Grade;

            submission.Feedback = request.Feedback;

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.SuccessResponse(submission.Id);
        }
    }
}
