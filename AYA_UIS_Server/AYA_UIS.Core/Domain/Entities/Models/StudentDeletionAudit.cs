namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Minimal administrative proof of a permanent student deletion executed
    /// from the Email Manager → Danger Zone. Holds only identifiers and counts
    /// (no academic data) so storage stays freed after the deletion.
    /// </summary>
    public class StudentDeletionAudit : BaseEntities<int>
    {
        public DateTime DeletedAt           { get; set; } = DateTime.UtcNow;
        public string   DeletedByAdminId    { get; set; } = string.Empty;
        public string   DeletedStudentCode  { get; set; } = string.Empty;
        public string   DeletedStudentName  { get; set; } = string.Empty;
        public string?  DeletedStudentEmail { get; set; }

        public int RegistrationsRemoved          { get; set; }
        public int FinalGradesRemoved            { get; set; }
        public int MidtermGradesRemoved          { get; set; }
        public int FinalGradeReviewsRemoved      { get; set; }
        public int AssignmentSubmissionsRemoved  { get; set; }
        public int SubmissionFilesRemoved        { get; set; }
        public int QuizAttemptsRemoved           { get; set; }
        public int QuizAnswersRemoved            { get; set; }
        public int NotificationsRemoved          { get; set; }
        public int CourseResultsRemoved          { get; set; }
        public int SemesterGpasRemoved           { get; set; }
        public int UserStudyYearsRemoved         { get; set; }
        public int CourseExceptionsRemoved       { get; set; }
        public int AdminCourseLocksRemoved       { get; set; }
        public int ResetSnapshotsRemoved         { get; set; }
        public int OtpRowsRemoved                { get; set; }

        /// <summary>"completed" | "failed"</summary>
        public string  Status       { get; set; } = "completed";
        public string? ErrorMessage { get; set; }
    }
}
