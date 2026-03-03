using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.CourseResultDtos
{
    public class StudentCourseResultDto
    {
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public bool IsPassed { get; set; }
        public decimal Grade { get; set; }
    }
}
