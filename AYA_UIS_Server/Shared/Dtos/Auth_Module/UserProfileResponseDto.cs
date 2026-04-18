using System.Text.Json.Serialization;

namespace Shared.Dtos.Auth_Module
{
    /// <summary>
    /// GET /api/user/profile response — returns EXACTLY the fields the frontend expects.
    /// Student-only fields (year, entryYear, gpa) are conditionally included.
    /// </summary>
    public class UserProfileResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        [JsonPropertyName("data")]
        public UserProfileDataDto Data { get; set; } = new();
    }

    public class UserProfileDataDto
    {
        [JsonPropertyName("user")]
        public UserProfileUserDto User { get; set; } = new();
    }

    /// <summary>
    /// Base user profile — shared across all roles.
    /// Student-specific fields (year, entryYear, gpa) are null for non-students
    /// and will be excluded from JSON output via JsonIgnoreCondition.WhenWritingNull.
    /// </summary>
    public class UserProfileUserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("dob")]
        public string? Dob { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("themeId")]
        public string ThemeId { get; set; } = "default";

        [JsonPropertyName("academicCode")]
        public string? AcademicCode { get; set; }
    }
}
