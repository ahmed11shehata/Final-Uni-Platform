using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AYA_UIS.Core.Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public string Academic_Code { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public Levels? Level { get; set; } // if he study eng will prep year any other will start from first year
        public int? TotalCredits { get; set; } // the total credits that the student has registered for in the current study year
        public int? AllowedCredits { get; set; } = 21; // the total credits updated every semester depend on gpa and the level of the student
        public decimal? TotalGPA { get; set; } = 0; // the gpa of the student updated every semester
        // if he study eng will prep year any other will start from first year second year he choose his specialization and the department will be the one of the specialization
        public string? Specialization { get; set; } // e.g., Computer Science, Information Systems, etc.
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<AcademicSchedule> AcademicSchedules { get; set; } = new List<AcademicSchedule>();
        public ICollection<CourseUpload> CourseUpload { get; set; } = new List<CourseUpload>();
        public ICollection<SemesterGPA> SemesterGPAs { get; set; } = new List<SemesterGPA>();
        public ICollection<UserStudyYear> UserStudyYears { get; set; } = new List<UserStudyYear>();

    }
}
