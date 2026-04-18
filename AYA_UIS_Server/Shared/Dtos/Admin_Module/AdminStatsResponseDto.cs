using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    /// <summary>GET /api/admin/stats response</summary>
    public class AdminStatsResponseDto
    {
        [JsonPropertyName("totalStudents")]
        public int TotalStudents { get; set; }

        [JsonPropertyName("totalRegistered")]
        public int TotalRegistered { get; set; }

        [JsonPropertyName("totalInstructors")]
        public int TotalInstructors { get; set; }

        [JsonPropertyName("activeCourses")]
        public int ActiveCourses { get; set; }

        [JsonPropertyName("registrationRate")]
        public int RegistrationRate { get; set; }

        [JsonPropertyName("trends")]
        public AdminTrendsDto Trends { get; set; } = new();
    }

    public class AdminTrendsDto
    {
        [JsonPropertyName("students")]
        public string Students { get; set; } = "+0%";

        [JsonPropertyName("registered")]
        public string Registered { get; set; } = "+0%";

        [JsonPropertyName("instructors")]
        public string Instructors { get; set; } = "+0";
    }
}
