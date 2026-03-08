using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace AYA_UIS.Application.Commands.CreateAssignment
{
    public class CreateAssignmentCommand : IRequest<Response<int>>
    {
        public CreateAssignmentDto AssignmentDto { get; set; }

        public IFormFile File { get; set; }

        public string InstructorId { get; set; }
    }
}
