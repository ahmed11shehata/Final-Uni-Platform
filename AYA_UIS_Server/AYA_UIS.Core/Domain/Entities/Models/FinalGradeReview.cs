using AYA_UIS.Core.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Admin's manual classification of a student's final-grade review status for the
    /// current term. A missing row implies the student is still in Progress.
    /// Absence of a row for a student with current-term registered courses means
    /// they are considered "progress" until the admin classifies them.
    /// </summary>
    [Index(nameof(StudentId), nameof(StudyYearId), nameof(SemesterId), IsUnique = true)]
    public class FinalGradeReview : BaseEntities<int>
    {
        public string StudentId { get; set; } = string.Empty;
        public User? Student { get; set; }

        public int StudyYearId { get; set; }
        public StudyYear? StudyYear { get; set; }

        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }

        /// <summary>"progress" | "not_completed" | "completed"</summary>
        public string Status { get; set; } = "progress";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
