using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abstraction.Contracts;
using AYA_UIS.Application.Commands.CreateAssignment;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class SubmitAssignmentCommandHandler
: IRequestHandler<SubmitAssignmentCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalFileService _fileService;

        public SubmitAssignmentCommandHandler(
            IUnitOfWork unitOfWork,
            ILocalFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<Response<int>> Handle(
            SubmitAssignmentCommand request,
            CancellationToken cancellationToken)
        {
            var assignment = await _unitOfWork.Assignments
                .GetByIdAsync(request.AssignmentId);

            if (assignment == null)
                return Response<int>.ErrorResponse("Assignment not found");

            if (DateTime.UtcNow > assignment.Deadline)
                return Response<int>.ErrorResponse("Deadline passed");

            var exists = await _unitOfWork.Assignments
                .SubmissionExists(request.AssignmentId, request.Academic_Code);

            if (exists)
                return Response<int>.ErrorResponse("Already submitted");

            var fileId = Guid.NewGuid().ToString();

            var fileUrl = await _fileService.UploadSubmissionFileAsync(
                request.File,
                fileId,
                request.AssignmentId,
                cancellationToken);

            var submission = new AssignmentSubmission
            {
                AssignmentId = request.AssignmentId,
                StudentId = request.Academic_Code,
                FileUrl = fileUrl,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.Assignments
                .AddSubmissionAsync(submission);

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.SuccessResponse(submission.Id);
        }
    }
}
