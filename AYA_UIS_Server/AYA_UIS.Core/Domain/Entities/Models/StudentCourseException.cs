using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class StudentCourseException :BaseEntities<int>
    {

        public string UserId { get; set; } = null!;
        public int CourseId { get; set; }

        public int StudyYearId { get; set; }
        public int SemesterId { get; set; }
    }
}
