using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class CreateAssignmentDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Points { get; set; }

        public DateTime Deadline { get; set; }

        public int CourseId { get; set; }
    }
}
