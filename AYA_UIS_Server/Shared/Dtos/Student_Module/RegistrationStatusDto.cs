using System.Text.Json.Serialization;

namespace Shared.Dtos.Student_Module
{
    public class RegisteredCourseItemDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }
    }

    public class RegistrationStatusDto
    {
        [JsonPropertyName("open")]
        public bool Open { get; set; }

        [JsonPropertyName("currentCredits")]
        public int CurrentCredits { get; set; }

        [JsonPropertyName("maxCredits")]
        public int MaxCredits { get; set; }

        [JsonPropertyName("openedPoolCredits")]
        public int OpenedPoolCredits { get; set; }

        [JsonPropertyName("registeredCourses")]
        public List<RegisteredCourseItemDto> RegisteredCourses { get; set; } = new();

        [JsonPropertyName("failedCourses")]
        public List<string> FailedCourses { get; set; } = new();

        [JsonPropertyName("lockedCourses")]
        public List<string> LockedCourses { get; set; } = new();

        [JsonPropertyName("currentYear")]
        public int CurrentYear { get; set; }

        [JsonPropertyName("semester")]
        public string? Semester { get; set; }

        [JsonPropertyName("academicYear")]
        public string? AcademicYear { get; set; }

        // Kept for backward compat
        [JsonPropertyName("allowedYears")]
        public List<int> AllowedYears { get; set; } = new();

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
