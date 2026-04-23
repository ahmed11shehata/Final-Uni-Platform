using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities;
using Abstraction.Contracts;
using AYA_UIS.Application.Commands.CreateAssignment;
using Domain.Contracts;
using MediatR;
using Shared.Respones;
using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class CreateAssignmentCommandHandler
: IRequestHandler<CreateAssignmentCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalFileService _fileService;

        public CreateAssignmentCommandHandler(
            IUnitOfWork unitOfWork,
            ILocalFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<Response<int>> Handle(
            CreateAssignmentCommand request,
            CancellationToken cancellationToken)
        {
            string fileUrl = string.Empty;
            if (request.File is not null)
            {
                var fileId = Guid.NewGuid().ToString();
                fileUrl = await _fileService.UploadAssignmentFileAsync(
                    request.File,
                    fileId,
                    request.AssignmentDto.CourseId,
                    cancellationToken);
            }

            var assignment = new Assignment
            {
                Title = request.AssignmentDto.Title,
                Description = request.AssignmentDto.Description,
                Points = request.AssignmentDto.Points,
                Deadline = request.AssignmentDto.Deadline,
                ReleaseDate = request.AssignmentDto.ReleaseDate,
                CourseId = request.AssignmentDto.CourseId,
                FileUrl = fileUrl,
                CreatedByUserId = request.InstructorId
            };

            await _unitOfWork.Assignments.AddAsync(assignment);

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.SuccessResponse(assignment.Id);
        }
    }
}
