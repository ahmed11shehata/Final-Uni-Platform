using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class AssignmentSubmissionDto
    {
        public int    Id        { get; set; }
        public string StudentId { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }

        public int? Grade { get; set; }

        public string Feedback { get; set; } = string.Empty;
    }
}

