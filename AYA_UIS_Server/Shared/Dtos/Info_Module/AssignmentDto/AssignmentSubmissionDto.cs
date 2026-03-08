using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class AssignmentSubmissionDto
    {
        public int Id { get; set; }

        public string StudentName { get; set; }

        public string FileUrl { get; set; }

        public DateTime SubmittedAt { get; set; }

        public int? Grade { get; set; }

        public string Feedback { get; set; }
    }
}
