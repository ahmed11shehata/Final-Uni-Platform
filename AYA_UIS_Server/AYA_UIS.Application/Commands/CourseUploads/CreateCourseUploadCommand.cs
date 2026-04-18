using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos.Info_Module.CourseUploadDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Commands.CourseUploads
{
    public class CreateCourseUploadCommand : IRequest<Response<int>>
    {
        public CreateCourseUploadDto CourseUploadDto { get; set; }
        public IFormFile? File { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}