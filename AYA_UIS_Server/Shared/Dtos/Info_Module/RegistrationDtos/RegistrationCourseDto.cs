using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;
using Shared.Dtos.Info_Module.CourseDtos;

namespace Shared.Dtos.Info_Module.RegistrationDtos
{
    public class RegistrationCourseDto
    {
        public int Id { get; set; }
        public RegistrationStatus Status { get; set; }
        public CourseProgress Progress { get; set; }
        public string? Reason { get; set; } // the reason for pending or canceling the registration
        public Grads Grade { get; set; } // null if the course is not yet completed, otherwise it holds the grade received
        public bool IsPassed { get; set; } // This property indicates whether the course has been passed
        public CourseDto Course { get; set; } = null!;
        // Convenience fields for frontend
        public int    CourseId   { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}
