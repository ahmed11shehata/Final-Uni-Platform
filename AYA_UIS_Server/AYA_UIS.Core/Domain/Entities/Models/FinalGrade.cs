using System.ComponentModel.DataAnnotations;
using AYA_UIS.Core.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// One final-grade record per student+course.
    /// Unique index prevents duplicate inserts (concurrent Add race).
    /// RowVersion enables optimistic concurrency detection on Update.
    /// </summary>
    [Index(nameof(StudentId), nameof(CourseId), IsUnique = true)]
    public class FinalGrade : BaseEntities<int>
    {
        public string StudentId { get; set; } = string.Empty;
        public User? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        /// <summary>Instructor-entered final exam score, 0–60.</summary>
        public int FinalScore { get; set; } = 0;

        /// <summary>Optional bonus added on top of coursework, 0–10.</summary>
        public int Bonus { get; set; } = 0;

        /// <summary>
        /// Admin-entered total (0–100) that overrides the computed cwTotal+FinalScore.
        /// Set when admin saves an Academic Setup grade for an actively-registered course.
        /// Cleared (null) when the instructor sets a FinalScore for this course.
        /// </summary>
        public int? AdminFinalTotal { get; set; }

        public bool Published { get; set; } = false;

        /// <summary>
        /// Optimistic concurrency token — SQL Server rowversion.
        /// EF Core auto-appends WHERE RowVersion = @orig on UPDATE,
        /// throwing DbUpdateConcurrencyException if another write landed first.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
