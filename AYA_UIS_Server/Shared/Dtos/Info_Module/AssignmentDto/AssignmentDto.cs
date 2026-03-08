using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class AssignmentDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Points { get; set; }

        public DateTime Deadline { get; set; }

        public string FileUrl { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public string InstructorName { get; set; }
    }
}
