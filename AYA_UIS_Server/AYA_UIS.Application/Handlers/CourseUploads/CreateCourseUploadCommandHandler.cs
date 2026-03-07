using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstraction.Contracts;
using AutoMapper;
using AYA_UIS.Application.Commands.CourseUploads;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.CourseUploads
{
    public class CreateCourseUploadCommandHandler : IRequestHandler<CreateCourseUploadCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        //private readonly ICloudinaryService _cloudinaryService;
        private readonly ILocalFileService _localFileService;
        private readonly IMapper _mapper;
        public CreateCourseUploadCommandHandler(IUnitOfWork unitOfWork, ILocalFileService localFileService, /*ICloudinaryService cloudinaryService,*/ IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            //_cloudinaryService = cloudinaryService;
            _localFileService = localFileService;
            _mapper = mapper;
        }
        public async Task<Response<int>> Handle(CreateCourseUploadCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseUploadDto.CourseId);
            if (course is null)                
                return Response<int>.ErrorResponse("Course not found");

            var fileId = Guid.NewGuid().ToString();

            //var fileUrl = await _cloudinaryService.UploadCourseFileAsync(request.File, fileId, course.Name, cancellationToken);


            var fileUrl = await _localFileService.UploadCourseFileAsync(
                   request.File,
                   fileId,
                   course.Name,
                   request.CourseUploadDto.Type,
                   cancellationToken);

            var courseUpload = _mapper.Map<CourseUpload>(request.CourseUploadDto);
            courseUpload.Url = fileUrl;
            courseUpload.UploadedAt = DateTime.UtcNow;
            courseUpload.FileId = fileId;
            courseUpload.UploadedByUserId = request.UserId;

            await _unitOfWork.CourseUploads.AddAsync(courseUpload);
            await _unitOfWork.SaveChangesAsync();

            return Response<int>.SuccessResponse(courseUpload.Id);
        }
    }
}