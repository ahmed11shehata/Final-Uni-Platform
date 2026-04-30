using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Pre-execution snapshot of one student's affected data, captured for
    /// audit / restore. PayloadJson contains the full pre-reset state of
    /// registrations, final grades, midterms, submissions and quiz attempts.
    /// </summary>
    public class AcademicYearResetSnapshot : BaseEntities<int>
    {
        public int ResetId { get; set; }
        public AcademicYearReset? Reset { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public User? Student { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        // Frozen labels for audit (human-readable)
        public string? SourceLevel    { get; set; }
        public string? SourceSemester { get; set; }
        public string? TargetLevel    { get; set; }
        public string? TargetSemester { get; set; }

        // Stable numeric source/target position used by the duplicate-reset guard.
        // SourceYearNum is the student's year-bucket (1-4), SourceSemesterNum is 1 or 2.
        // Legacy snapshots written before these columns existed will have NULL here
        // and are intentionally ignored by the duplicate check.
        public int? SourceYearNum     { get; set; }
        public int? SourceSemesterNum { get; set; }
        public int? TargetYearNum     { get; set; }
        public int? TargetSemesterNum { get; set; }

        // Counts pre-reset
        public int RegistrationsCount { get; set; }
        public int FinalGradesCount   { get; set; }
        public int SubmissionsCount   { get; set; }
        public int QuizAttemptsCount  { get; set; }

        /// <summary>
        /// JSON blob containing the full pre-reset payload for this student.
        /// Stored as NVARCHAR(MAX) so a restore script could rehydrate state.
        /// </summary>
        public string PayloadJson { get; set; } = "{}";
    }
}
