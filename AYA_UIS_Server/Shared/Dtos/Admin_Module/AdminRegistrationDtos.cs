using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    // ── Opened course entry for per-bucket seat configuration ─────
    public class OpenedCourseEntryDto
    {
        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("availableSeats")]
        public object? AvailableSeats { get; set; }   // int or "unlimited"

        [JsonPropertyName("isUnlimitedSeats")]
        public bool IsUnlimitedSeats { get; set; } = true;
    }

    // ── GET /api/admin/registration/status ─────────────────────
    public class AdminRegistrationStatusDto
    {
        [JsonPropertyName("isOpen")]
        public bool IsOpen { get; set; }

        [JsonPropertyName("semester")]
        public string? Semester { get; set; }

        [JsonPropertyName("academicYear")]
        public string? AcademicYear { get; set; }

        [JsonPropertyName("startDate")]
        public string? StartDate { get; set; }

        [JsonPropertyName("deadline")]
        public string? Deadline { get; set; }

        [JsonPropertyName("openedCoursesByYear")]
        public Dictionary<string, List<OpenedCourseEntryDto>> OpenedCoursesByYear { get; set; } = new();

        [JsonPropertyName("maxCredits")]
        public int? MaxCredits { get; set; }
    }

    // ── Internal JSON model stored in RegistrationSettings.OpenedCoursesByYear ──
    public class OpenedCourseEntryInternal
    {
        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("availableSeats")]
        public int? AvailableSeats { get; set; }  // null = unlimited

        [JsonPropertyName("isUnlimitedSeats")]
        public bool IsUnlimitedSeats { get; set; } = true;
    }

    // ── POST /api/admin/registration/start ─────────────────────
    public class StartRegistrationDto
    {
        [Required]
        [JsonPropertyName("semester")]
        public string Semester { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("academicYear")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("startDate")]
        public string StartDate { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("deadline")]
        public string Deadline { get; set; } = string.Empty;

        /// <summary>Per-year opened courses with seats: {"1":[{...}],"2":[{...}],...}</summary>
        [Required]
        [JsonPropertyName("openedCoursesByYear")]
        public Dictionary<string, List<OpenedCourseEntryInternal>> OpenedCoursesByYear { get; set; } = new();

        [JsonPropertyName("maxCredits")]
        public int? MaxCredits { get; set; }
    }

    // ── PUT /api/admin/courses/settings ────────────────────────
    public class AdminCourseSettingsDto
    {
        [JsonPropertyName("openYears")]
        public List<int> OpenYears { get; set; } = new();

        [JsonPropertyName("enabledCourses")]
        public List<string> EnabledCourses { get; set; } = new();
    }

    // ── Student Control DTOs ──────────────────────────────────
    public class AdminStudentDetailDto
    {
        [JsonPropertyName("student")]
        public AdminStudentProfileDto Student { get; set; } = new();

        [JsonPropertyName("standing")]
        public AcademicStandingDto Standing { get; set; } = new();

        [JsonPropertyName("registeredCourses")]
        public List<AdminStudentCourseDto> RegisteredCourses { get; set; } = new();

        [JsonPropertyName("completedCourses")]
        public List<AdminStudentCourseDto> CompletedCourses { get; set; } = new();
    }

    public class AdminStudentProfileDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("academicCode")]
        public string? AcademicCode { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("year")]
        public string? Year { get; set; }

        [JsonPropertyName("entryYear")]
        public string? EntryYear { get; set; }

        [JsonPropertyName("gpa")]
        public decimal? Gpa { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }

    public class AcademicStandingDto
    {
        [JsonPropertyName("standingId")]
        public string StandingId { get; set; } = string.Empty;

        [JsonPropertyName("gpa")]
        public decimal Gpa { get; set; }

        [JsonPropertyName("maxCredits")]
        public int MaxCredits { get; set; }

        [JsonPropertyName("mustRetakeFirst")]
        public bool MustRetakeFirst { get; set; }

        [JsonPropertyName("canOnlyRetake")]
        public bool CanOnlyRetake { get; set; }

        [JsonPropertyName("isNewStudent")]
        public bool IsNewStudent { get; set; }
    }

    public class AdminStudentCourseDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }
    }

    public class AdminAddCourseDto
    {
        [Required]
        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;
    }

    public class AdminMaxCreditsDto
    {
        [Required]
        [Range(0, 30)]
        [JsonPropertyName("maxCredits")]
        public int MaxCredits { get; set; }
    }

    // ── Admin Courses List response ───────────────────────────
    public class AdminCourseListItemDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("semester")]
        public string? Semester { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("prerequisites")]
        public List<string> Prerequisites { get; set; } = new();
    }
}
