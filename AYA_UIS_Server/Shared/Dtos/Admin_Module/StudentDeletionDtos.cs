using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    /// <summary>Read-only preview of a student about to be permanently deleted.</summary>
    public class StudentDeletionPreviewDto
    {
        [JsonPropertyName("academicCode")]
        public string AcademicCode { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("year")]
        public string? Year { get; set; }

        [JsonPropertyName("semester")]
        public string? Semester { get; set; }

        [JsonPropertyName("registeredCoursesCount")]
        public int RegisteredCoursesCount { get; set; }

        [JsonPropertyName("totalRegistrations")]
        public int TotalRegistrations { get; set; }

        [JsonPropertyName("submissionsCount")]
        public int SubmissionsCount { get; set; }

        [JsonPropertyName("quizAttemptsCount")]
        public int QuizAttemptsCount { get; set; }
    }

    public class StudentDeletionExecuteRequestDto
    {
        [JsonPropertyName("academicCode")]
        public string AcademicCode { get; set; } = string.Empty;

        /// <summary>Must equal "StudentDelete@123#" (configurable via StudentDelete:Password).</summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Admin must re-type the academic code to confirm the target.</summary>
        [JsonPropertyName("confirmAcademicCode")]
        public string ConfirmAcademicCode { get; set; } = string.Empty;
    }

    public class StudentDeletionResultDto
    {
        [JsonPropertyName("auditId")]
        public int AuditId { get; set; }

        [JsonPropertyName("deletedStudentCode")]
        public string DeletedStudentCode { get; set; } = string.Empty;

        [JsonPropertyName("deletedStudentName")]
        public string DeletedStudentName { get; set; } = string.Empty;

        [JsonPropertyName("counts")]
        public StudentDeletionCountsDto Counts { get; set; } = new();
    }

    public class StudentDeletionCountsDto
    {
        [JsonPropertyName("registrationsRemoved")]         public int RegistrationsRemoved          { get; set; }
        [JsonPropertyName("finalGradesRemoved")]           public int FinalGradesRemoved            { get; set; }
        [JsonPropertyName("midtermGradesRemoved")]         public int MidtermGradesRemoved          { get; set; }
        [JsonPropertyName("finalGradeReviewsRemoved")]     public int FinalGradeReviewsRemoved      { get; set; }
        [JsonPropertyName("assignmentSubmissionsRemoved")] public int AssignmentSubmissionsRemoved  { get; set; }
        [JsonPropertyName("submissionFilesRemoved")]       public int SubmissionFilesRemoved        { get; set; }
        [JsonPropertyName("quizAttemptsRemoved")]          public int QuizAttemptsRemoved           { get; set; }
        [JsonPropertyName("quizAnswersRemoved")]           public int QuizAnswersRemoved            { get; set; }
        [JsonPropertyName("notificationsRemoved")]         public int NotificationsRemoved          { get; set; }
        [JsonPropertyName("courseResultsRemoved")]         public int CourseResultsRemoved          { get; set; }
        [JsonPropertyName("semesterGpasRemoved")]          public int SemesterGpasRemoved           { get; set; }
        [JsonPropertyName("userStudyYearsRemoved")]        public int UserStudyYearsRemoved         { get; set; }
        [JsonPropertyName("courseExceptionsRemoved")]      public int CourseExceptionsRemoved       { get; set; }
        [JsonPropertyName("adminCourseLocksRemoved")]      public int AdminCourseLocksRemoved       { get; set; }
        [JsonPropertyName("resetSnapshotsRemoved")]        public int ResetSnapshotsRemoved         { get; set; }
        [JsonPropertyName("otpRowsRemoved")]               public int OtpRowsRemoved                { get; set; }
    }
}
