using System.Text.Json.Serialization;

namespace Shared.Dtos.Student_Module
{
    public class RegistrationCoursesDto
    {
        [JsonPropertyName("yearCounts")]
        public Dictionary<string, int>? YearCounts { get; set; }

        [JsonPropertyName("courses")]
        public List<RegistrationCourseDto> Courses { get; set; } = new();

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Matches frontend CourseRegistrationPage card shape.
    /// </summary>
    public class RegistrationCourseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("instructor")]
        public string Instructor { get; set; } = string.Empty;

        [JsonPropertyName("schedule")]
        public string Schedule { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        // ── Seats ──
        [JsonPropertyName("capacity")]
        public int? Capacity { get; set; }

        [JsonPropertyName("enrolled")]
        public int Enrolled { get; set; }

        [JsonPropertyName("remainingSeats")]
        public int? RemainingSeats { get; set; }

        [JsonPropertyName("availableSeats")]
        public object? AvailableSeats { get; set; }   // int or "unlimited"

        [JsonPropertyName("isUnlimitedSeats")]
        public bool IsUnlimitedSeats { get; set; }

        // ── Status ──
        /// <summary>available | registered | completed | unavailable | locked | full</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("prereqs")]
        public List<string> Prereqs { get; set; } = new();

        [JsonPropertyName("missingPrerequisites")]
        public List<string>? MissingPrerequisites { get; set; }

        /// <summary>
        /// Per-prerequisite breakdown (code, name, passed flag) so the frontend
        /// can render the Prerequisite tab with clear "Passed / Missing" labels.
        /// </summary>
        [JsonPropertyName("prerequisiteDetails")]
        public List<PrerequisiteDetailDto>? PrerequisiteDetails { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;

        [JsonPropertyName("pattern")]
        public string Pattern { get; set; } = string.Empty;

        // ── Flags ──
        [JsonPropertyName("retake")]
        public bool Retake { get; set; }

        [JsonPropertyName("canRegister")]
        public bool CanRegister { get; set; }

        [JsonPropertyName("isExceptionCourse")]
        public bool IsExceptionCourse { get; set; }

        [JsonPropertyName("isAdminLocked")]
        public bool IsAdminLocked { get; set; }

        // ── Credits context ──
        [JsonPropertyName("currentCredits")]
        public int CurrentCredits { get; set; }

        [JsonPropertyName("currentMaxCredits")]
        public int CurrentMaxCredits { get; set; }

        [JsonPropertyName("wouldExceedCredits")]
        public bool WouldExceedCredits { get; set; }

        // ── Year context ──
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("courseYear")]
        public int? CourseYear { get; set; }

        // Kept for backward compat but deprecated
        [JsonPropertyName("lockReason")]
        public string? LockReason { get; set; }
    }

    /// <summary>
    /// One prerequisite course with its pass/missing status for the current student.
    /// </summary>
    public class PrerequisiteDetailDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>True if the student has already passed this prerequisite.</summary>
        [JsonPropertyName("passed")]
        public bool Passed { get; set; }
    }
}
