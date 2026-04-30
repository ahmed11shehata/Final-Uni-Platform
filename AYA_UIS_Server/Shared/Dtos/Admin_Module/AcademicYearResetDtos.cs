using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    // ═════════════════════════════════════════════════════════════
    // POST /api/admin/academic-reset/preview — Request
    // ═════════════════════════════════════════════════════════════

    public class AcademicYearResetPreviewRequestDto
    {
        [JsonPropertyName("studentIds")]
        public List<string> StudentIds { get; set; } = new();

        [JsonPropertyName("selectAll")]
        public bool SelectAll { get; set; }
    }

    // ═════════════════════════════════════════════════════════════
    // POST /api/admin/academic-reset/preview — Response
    // ═════════════════════════════════════════════════════════════

    public class AcademicYearResetPreviewResponseDto
    {
        [JsonPropertyName("selectedCount")]
        public int SelectedCount { get; set; }

        [JsonPropertyName("requiresForceReset")]
        public bool RequiresForceReset { get; set; }

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();

        [JsonPropertyName("totals")]
        public AcademicYearResetTotalsDto Totals { get; set; } = new();

        [JsonPropertyName("perStudent")]
        public List<AcademicYearResetStudentPreviewDto> PerStudent { get; set; } = new();
    }

    public class AcademicYearResetTotalsDto
    {
        [JsonPropertyName("registeredCourses")]
        public int RegisteredCourses { get; set; }

        [JsonPropertyName("passedCourses")]
        public int PassedCourses { get; set; }

        [JsonPropertyName("failedCourses")]
        public int FailedCourses { get; set; }

        [JsonPropertyName("unassignedGrades")]
        public int UnassignedGrades { get; set; }

        [JsonPropertyName("notCompletedReviewCount")]
        public int NotCompletedReviewCount { get; set; }

        [JsonPropertyName("alreadyResetCount")]
        public int AlreadyResetCount { get; set; }

        [JsonPropertyName("noRegistrationsCount")]
        public int NoRegistrationsCount { get; set; }
    }

    public class AcademicYearResetStudentPreviewDto
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [JsonPropertyName("academicCode")]
        public string AcademicCode { get; set; } = string.Empty;

        [JsonPropertyName("currentLevel")]
        public string CurrentLevel { get; set; } = string.Empty;

        [JsonPropertyName("currentSemester")]
        public int CurrentSemester { get; set; }

        [JsonPropertyName("targetLevel")]
        public string TargetLevel { get; set; } = string.Empty;

        [JsonPropertyName("targetSemester")]
        public int TargetSemester { get; set; }

        [JsonPropertyName("registeredCount")]
        public int RegisteredCount { get; set; }

        [JsonPropertyName("passedCount")]
        public int PassedCount { get; set; }

        [JsonPropertyName("failedCount")]
        public int FailedCount { get; set; }

        [JsonPropertyName("unassignedCount")]
        public int UnassignedCount { get; set; }

        [JsonPropertyName("reviewStatus")]
        public string ReviewStatus { get; set; } = "progress";

        [JsonPropertyName("alreadyReset")]
        public bool AlreadyReset { get; set; }

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();
    }

    // ═════════════════════════════════════════════════════════════
    // POST /api/admin/academic-reset/execute — Request
    // ═════════════════════════════════════════════════════════════

    public class AcademicYearResetExecuteRequestDto
    {
        [JsonPropertyName("studentIds")]
        public List<string> StudentIds { get; set; } = new();

        [JsonPropertyName("selectAll")]
        public bool SelectAll { get; set; }

        [Required]
        [JsonPropertyName("confirmationText")]
        public string ConfirmationText { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("resetPassword")]
        public string ResetPassword { get; set; } = string.Empty;

        [JsonPropertyName("forceReset")]
        public bool ForceReset { get; set; }
    }

    // ═════════════════════════════════════════════════════════════
    // POST /api/admin/academic-reset/execute — Response
    // ═════════════════════════════════════════════════════════════

    public class AcademicYearResetExecuteResponseDto
    {
        [JsonPropertyName("resetId")]
        public int ResetId { get; set; }

        [JsonPropertyName("studentsReset")]
        public int StudentsReset { get; set; }

        [JsonPropertyName("archivedRegistrations")]
        public int ArchivedRegistrations { get; set; }

        [JsonPropertyName("passedCount")]
        public int PassedCount { get; set; }

        [JsonPropertyName("failedCount")]
        public int FailedCount { get; set; }

        [JsonPropertyName("unassignedFailedCount")]
        public int UnassignedFailedCount { get; set; }

        [JsonPropertyName("finalGradesPurged")]
        public int FinalGradesPurged { get; set; }

        [JsonPropertyName("quizAttemptsPurged")]
        public int QuizAttemptsPurged { get; set; }

        [JsonPropertyName("submissionsPurged")]
        public int SubmissionsPurged { get; set; }

        [JsonPropertyName("midtermsPurged")]
        public int MidtermsPurged { get; set; }

        [JsonPropertyName("notificationsSent")]
        public int NotificationsSent { get; set; }

        [JsonPropertyName("limitations")]
        public List<string> Limitations { get; set; } = new();
    }
}
