using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class Registration : BaseEntities<int>
    {
        public RegistrationStatus Status { get; set; }
        public CourseProgress Progress { get; set; }
        public string? Reason { get; set; } // the reason for pending or canceling the registration
        public Grads? Grade { get; set; } // null if the course is not yet completed, otherwise it holds the grade received
        public bool IsPassed { get; set; } // This property indicates whether the course has been passed by the student or not. It can be used to determine if the student has met the prerequisites for other courses. 
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public int StudyYearId { get; set; }
        public StudyYear StudyYear { get; set; } = null!;
        public int SemesterId { get; set; }
        public Semester Semester { get; set; } = null!;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Academic Setup / Equivalency
        public bool IsEquivalency { get; set; } = false;
        public int? NumericTotal { get; set; } // Raw numeric score (0-100) for equivalency courses
        public int? TranscriptYear { get; set; } // Admin-selected academic year (1-4) for equivalency records; overrides catalog year

        // Academic Year Reset / Archive
        // True after an Academic Year Reset moves this row out of the student's
        // current term. Archived rows must not appear in any "current" view but
        // remain in the transcript history when NumericTotal / Grade are populated.
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
    }
}