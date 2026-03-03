using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.CourseDtos
{
    public class GrantCourseExceptionDto
    {
        public string AcademicCode { get; set; } = null!;
        public int CourseId { get; set; }
        public int StudyYearId { get; set; }
        public int SemesterId { get; set; }
    }
}
