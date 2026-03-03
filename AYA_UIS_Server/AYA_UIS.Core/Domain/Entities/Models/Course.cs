using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class Course : BaseEntities<int>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int DepartmentId { get; set; }
        public CourseStatus Status { get; set; }
        public Department Department { get; set; } = null!;
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<CourseUpload> CourseUpload { get; set; } = new List<CourseUpload>();

        public ICollection<CoursePrerequisite> PrerequisiteFor { get; set; } = new List<CoursePrerequisite>();
        public ICollection<CoursePrerequisite> DependentCourses { get; set; } = new List<CoursePrerequisite>();
    }
}