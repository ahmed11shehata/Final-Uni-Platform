using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    // ═════════════════════════════════════════════════════════════
    // GET /api/admin/student/{studentId}/academic-setup
    // ═════════════════════════════════════════════════════════════

    public class AcademicSetupResponseDto
    {
        [JsonPropertyName("student")]
        public AcademicSetupStudentDto Student { get; set; } = new();

        [JsonPropertyName("academicSetup")]
        public AcademicSetupDataDto AcademicSetup { get; set; } = new();
    }

    public class AcademicSetupStudentDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("currentYear")]
        public int CurrentYear { get; set; }

        [JsonPropertyName("gpa")]
        public decimal Gpa { get; set; }

        [JsonPropertyName("totalCreditsEarned")]
        public int TotalCreditsEarned { get; set; }

        [JsonPropertyName("standing")]
        public AcademicStandingDto Standing { get; set; } = new();
    }

    public class AcademicSetupDataDto
    {
        [JsonPropertyName("currentYear")]
        public int CurrentYear { get; set; }

        [JsonPropertyName("years")]
        public Dictionary<string, AcademicSetupYearDto> Years { get; set; } = new();
    }

    public class AcademicSetupYearDto
    {
        [JsonPropertyName("semesters")]
        public Dictionary<string, List<AcademicSetupCourseDto>> Semesters { get; set; } = new();
    }

    public class AcademicSetupCourseDto
    {
        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        [JsonPropertyName("selected")]
        public bool Selected { get; set; }

        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }

        [JsonPropertyName("gpaPoints")]
        public decimal? GpaPoints { get; set; }

        [JsonPropertyName("isEquivalency")]
        public bool IsEquivalency { get; set; }
    }

    // ═════════════════════════════════════════════════════════════
    // PUT /api/admin/student/{studentId}/academic-setup — Request
    // ═════════════════════════════════════════════════════════════

    public class AcademicSetupSaveRequestDto
    {
        [Required]
        [JsonPropertyName("currentYear")]
        public int CurrentYear { get; set; }

        [Required]
        [JsonPropertyName("years")]
        public Dictionary<string, AcademicSetupYearSaveDto> Years { get; set; } = new();
    }

    public class AcademicSetupYearSaveDto
    {
        [JsonPropertyName("completedCourses")]
        public List<AcademicSetupCourseSaveDto> CompletedCourses { get; set; } = new();
    }

    public class AcademicSetupCourseSaveDto
    {
        [Required]
        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>
        /// Optional: admin-specified semester (1 or 2).
        /// When supplied the course is stored in this semester;
        /// otherwise the semester is derived from CourseOffering data.
        /// </summary>
        [JsonPropertyName("semester")]
        public int? Semester { get; set; }
    }

    // ═════════════════════════════════════════════════════════════
    // PUT — Success Response
    // ═════════════════════════════════════════════════════════════

    public class AcademicSetupSaveResultDto
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("currentYear")]
        public int CurrentYear { get; set; }

        [JsonPropertyName("gpa")]
        public decimal Gpa { get; set; }

        [JsonPropertyName("totalCreditsEarned")]
        public int TotalCreditsEarned { get; set; }

        [JsonPropertyName("standing")]
        public AcademicStandingDto Standing { get; set; } = new();

        [JsonPropertyName("completedCourses")]
        public List<AcademicSetupCompletedCourseDto> CompletedCourses { get; set; } = new();
    }

    public class AcademicSetupCompletedCourseDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("semester")]
        public int Semester { get; set; }

        [JsonPropertyName("grade")]
        public string Grade { get; set; } = string.Empty;

        [JsonPropertyName("gpaPoints")]
        public decimal GpaPoints { get; set; }

        [JsonPropertyName("isEquivalency")]
        public bool IsEquivalency { get; set; } = true;
    }

    // ═════════════════════════════════════════════════════════════
    // GET /api/student/transcript — student's own completed transcript
    // Returns ONLY courses with real admin-assigned grades (equivalency
    // registrations with IsPassed=true and a NumericTotal).
    // Does NOT include ungraded curriculum courses.
    // ═════════════════════════════════════════════════════════════

    public class StudentTranscriptResponseDto
    {
        [JsonPropertyName("student")]
        public AcademicSetupStudentDto Student { get; set; } = new();

        /// <summary>
        /// Flat list of completed/graded courses only.
        /// Frontend groups these by year + semester.
        /// </summary>
        [JsonPropertyName("completedCourses")]
        public List<TranscriptCourseDto> CompletedCourses { get; set; } = new();
    }

    public class TranscriptCourseDto
    {
        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("semester")]
        public int Semester { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("grade")]
        public string Grade { get; set; } = string.Empty;

        [JsonPropertyName("gpaPoints")]
        public decimal GpaPoints { get; set; }
    }
}
