using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class CourseResult
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public bool IsPassed { get; set; }
        public decimal Grade { get; set; }   // 0 – 100

        public int StudyYearId { get; set; }
        public StudyYear StudyYear { get; set; } = null!;
    }
}
