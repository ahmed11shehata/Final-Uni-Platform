using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Respones;

namespace AYA_UIS.Application.Commands.CreateAssignment
{
    public class SubmitAssignmentCommand : IRequest<Response<int>>
    {
        public int AssignmentId { get; set; }

        public string? Academic_Code { get; set; } = string.Empty;

        public IFormFile? File { get; set; }
    }
}
