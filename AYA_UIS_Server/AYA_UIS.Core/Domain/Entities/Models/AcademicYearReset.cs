using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Audit log for an Academic Year Reset operation. One row per reset.
    /// </summary>
    public class AcademicYearReset : BaseEntities<int>
    {
        public string AdminId { get; set; } = string.Empty;
        public User? Admin { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        public int  StudentsCount   { get; set; }
        public bool ForceReset      { get; set; }
        public bool SelectAll       { get; set; }

        // Source / target term labels (frozen at execution time)
        public int? SourceStudyYearId { get; set; }
        public int? SourceSemesterId  { get; set; }
        public int? TargetStudyYearId { get; set; }
        public int? TargetSemesterId  { get; set; }
        public string? SourceTerm     { get; set; } // e.g. "2025-2026 / Semester 1"
        public string? TargetTerm     { get; set; }

        public int ArchivedRegistrations { get; set; }
        public int PassedCount           { get; set; }
        public int FailedCount           { get; set; }
        public int UnassignedFailedCount { get; set; }
        public int FinalGradesPurged     { get; set; }
        public int QuizAttemptsPurged    { get; set; }
        public int SubmissionsPurged     { get; set; }
        public int MidtermsPurged        { get; set; }
        public int NotificationsSent     { get; set; }

        public string? SummaryJson { get; set; }
    }
}
