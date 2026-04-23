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

        public string Feedback { get; set; } = string.Empty;

        /// <summary>
        /// Explicit accept/reject flag.
        /// true  = accept → auto-assign full assignment points, Status = "Accepted"
        /// false = reject → Grade = 0, Status = "Rejected", RejectionReason stored
        /// null  = legacy (use Grade as-is)
        /// </summary>
        public bool? IsAccepted { get; set; }

        public string? RejectionReason { get; set; }
    }
}
