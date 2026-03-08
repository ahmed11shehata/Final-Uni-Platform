using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Commands.Assignment
{
    public class GradeSubmissionCommand : IRequest<Response<int>>
    {
        public int SubmissionId { get; set; }

        public int Grade { get; set; }

        public string Feedback { get; set; }
    }
}
