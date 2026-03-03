using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.CourseResultDtos
{
    public class AddStudentResultsDto
    {
        public string AcademicCode { get; set; } = null!;
        public int StudyYearId { get; set; }

        public List<CourseResultItemDto> Results { get; set; } = new();
    }
}
