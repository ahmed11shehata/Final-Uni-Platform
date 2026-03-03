using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.CourseDtos
{
    public class OpenCoursesForLevelDto
    {
        public int StudyYearId { get; set; }
        public int SemesterId { get; set; }
        public Levels Level { get; set; }

        public List<int> CourseIds { get; set; } = new();
    }
}
