using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    // ── Preview ──────────────────────────────────────────────────
    public class MaterialResetPreviewRequestDto
    {
        [JsonPropertyName("courseIds")]
        public List<int> CourseIds { get; set; } = new();

        [JsonPropertyName("selectAll")]
        public bool SelectAll { get; set; }
    }

    public class MaterialResetPreviewResponseDto
    {
        [JsonPropertyName("selectedCourseCount")]
        public int SelectedCourseCount { get; set; }

        [JsonPropertyName("totals")]
        public MaterialResetTotalsDto Totals { get; set; } = new();

        [JsonPropertyName("perCourse")]
        public List<MaterialResetCourseImpactDto> PerCourse { get; set; } = new();

        /// <summary>
        /// Pending submissions across the selected courses. If non-empty, execution is blocked
        /// until each one is accepted/rejected.
        /// </summary>
        [JsonPropertyName("pendingSubmissions")]
        public List<MaterialResetPendingSubmissionDto> PendingSubmissions { get; set; } = new();

        [JsonPropertyName("blocked")]
        public bool Blocked { get; set; }

        [JsonPropertyName("blockReason")]
        public string? BlockReason { get; set; }
    }

    public class MaterialResetTotalsDto
    {
        [JsonPropertyName("assignments")]
        public int Assignments { get; set; }

        [JsonPropertyName("quizzes")]
        public int Quizzes { get; set; }

        [JsonPropertyName("lectures")]
        public int Lectures { get; set; }

        [JsonPropertyName("pendingSubmissions")]
        public int PendingSubmissions { get; set; }

        [JsonPropertyName("instructorsAffected")]
        public int InstructorsAffected { get; set; }
    }

    public class MaterialResetCourseImpactDto
    {
        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("assignments")]
        public int Assignments { get; set; }

        [JsonPropertyName("quizzes")]
        public int Quizzes { get; set; }

        [JsonPropertyName("lectures")]
        public int Lectures { get; set; }

        [JsonPropertyName("pendingSubmissions")]
        public int PendingSubmissions { get; set; }

        [JsonPropertyName("hasMaterial")]
        public bool HasMaterial { get; set; }
    }

    public class MaterialResetPendingSubmissionDto
    {
        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("assignmentId")]
        public int AssignmentId { get; set; }

        [JsonPropertyName("assignmentTitle")]
        public string AssignmentTitle { get; set; } = string.Empty;

        [JsonPropertyName("pendingCount")]
        public int PendingCount { get; set; }

        [JsonPropertyName("studentCodes")]
        public List<string> StudentCodes { get; set; } = new();

        [JsonPropertyName("studentNames")]
        public List<string> StudentNames { get; set; } = new();
    }

    // ── Execute ──────────────────────────────────────────────────
    public class MaterialResetExecuteRequestDto
    {
        [JsonPropertyName("courseIds")]
        public List<int> CourseIds { get; set; } = new();

        [JsonPropertyName("selectAll")]
        public bool SelectAll { get; set; }

        /// <summary>Required strong confirmation — must equal "Material@123#".</summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class MaterialResetExecuteResponseDto
    {
        [JsonPropertyName("batchId")]
        public int BatchId { get; set; }

        [JsonPropertyName("selectedCourseCount")]
        public int SelectedCourseCount { get; set; }

        [JsonPropertyName("counts")]
        public MaterialResetExecuteCountsDto Counts { get; set; } = new();

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();

        [JsonPropertyName("perCourse")]
        public List<MaterialResetCourseImpactDto> PerCourse { get; set; } = new();
    }

    public class MaterialResetExecuteCountsDto
    {
        [JsonPropertyName("assignmentsArchived")]
        public int AssignmentsArchived { get; set; }

        [JsonPropertyName("assignmentFilesPurged")]
        public int AssignmentFilesPurged { get; set; }

        [JsonPropertyName("submissionFilesPurged")]
        public int SubmissionFilesPurged { get; set; }

        [JsonPropertyName("quizzesArchived")]
        public int QuizzesArchived { get; set; }

        [JsonPropertyName("lecturesDeleted")]
        public int LecturesDeleted { get; set; }

        [JsonPropertyName("lectureFilesDeleted")]
        public int LectureFilesDeleted { get; set; }

        [JsonPropertyName("instructorsNotified")]
        public int InstructorsNotified { get; set; }
    }
}
