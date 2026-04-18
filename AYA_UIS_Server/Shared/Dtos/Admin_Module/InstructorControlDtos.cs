using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    public class InstructorItemDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }

    public class InstructorCourseItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("assignedInstructors")]
        public List<InstructorItemDto> AssignedInstructors { get; set; } = [];
    }

    public class InstructorControlDto
    {
        [JsonPropertyName("isOpen")]
        public bool IsOpen { get; set; }

        [JsonPropertyName("courses")]
        public List<InstructorCourseItemDto> Courses { get; set; } = [];

        [JsonPropertyName("allInstructors")]
        public List<InstructorItemDto> AllInstructors { get; set; } = [];
    }

    public class AssignInstructorsDto
    {
        [JsonPropertyName("instructorIds")]
        public List<string> InstructorIds { get; set; } = [];
    }
}
