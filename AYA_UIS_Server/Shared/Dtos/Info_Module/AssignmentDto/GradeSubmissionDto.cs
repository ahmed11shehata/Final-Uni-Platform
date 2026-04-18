using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class GradeSubmissionDto
    {
        public int SubmissionId { get; set; }

        public int Grade { get; set; }

        public string Feedback { get; set; } = string.Empty;
    }
}
