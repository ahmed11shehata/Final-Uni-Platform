using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Audit log row for one Admin "Reset Material" batch.
    /// Distinct from <see cref="AcademicYearReset"/> (year-end student reset).
    /// </summary>
    public class MaterialReset : BaseEntities<int>
    {
        public string CreatedById { get; set; } = string.Empty;
        public User?  CreatedBy   { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int SelectedCourseCount         { get; set; }
        public int AssignmentCount             { get; set; }
        public int QuizCount                   { get; set; }
        public int LectureCount                { get; set; }
        public int SubmissionFilePurgedCount   { get; set; }
        public int InstructorsNotified         { get; set; }

        /// <summary>"completed" | "blocked" | "failed"</summary>
        public string Status { get; set; } = "completed";

        public string? ErrorMessage { get; set; }

        /// <summary>JSON snapshot of selection + per-course counters for audit/forensics.</summary>
        public string? SummaryJson { get; set; }
    }
}
