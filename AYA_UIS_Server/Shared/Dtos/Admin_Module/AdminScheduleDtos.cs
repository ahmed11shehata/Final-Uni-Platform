using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    // ── POST /api/admin/schedule/sessions ──────────────────────
    /// <summary>
    /// Frontend AdminSchedulePage session creation shape.
    /// Accepts course code + name directly.
    /// </summary>
    public class CreateSessionDto
    {
        [Required]
        [Range(1, 4)]
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [Required]
        [RegularExpression("^(A|B)$", ErrorMessage = "Group must be 'A' or 'B'.")]
        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Saturday|Sunday|Monday|Tuesday|Wednesday|Thursday)$")]
        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("start")]
        public double Start { get; set; }

        [Required]
        [JsonPropertyName("end")]
        public double End { get; set; }

        [Required]
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Lecture|Section|Lab)$", ErrorMessage = "Type must be 'Lecture', 'Section', or 'Lab'.")]
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("room")]
        public string Room { get; set; } = string.Empty;

        [JsonPropertyName("instructor")]
        public string Instructor { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;

        // Backward compat — old code may send these
        [JsonPropertyName("courseId")]
        public int? CourseId { get; set; }
        [JsonPropertyName("startTime")]
        public double? StartTime { get; set; }
        [JsonPropertyName("endTime")]
        public double? EndTime { get; set; }
    }

    /// <summary>
    /// Frontend weekly session response shape.
    /// </summary>
    public class SessionResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("group")]
        public string Group { get; set; } = string.Empty;

        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty;

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("room")]
        public string Room { get; set; } = string.Empty;

        [JsonPropertyName("instructor")]
        public string Instructor { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;
    }

    // ── POST /api/admin/schedule/exams ─────────────────────────
    /// <summary>
    /// Frontend AdminSchedulePage exam creation shape.
    /// </summary>
    public class CreateExamDto
    {
        [Required]
        [Range(1, 4)]
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [Required]
        [RegularExpression("^(midterm|final)$", ErrorMessage = "Type must be 'midterm' or 'final'.")]
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be YYYY-MM-DD.")]
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("hall")]
        public string Hall { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;

        // Backward compat
        [JsonPropertyName("courseId")]
        public int? CourseId { get; set; }
        [JsonPropertyName("startTime")]
        public double? StartTime { get; set; }
        [JsonPropertyName("location")]
        public string? Location { get; set; }
    }

    /// <summary>
    /// Frontend exam response shape.
    /// </summary>
    public class ExamResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("hall")]
        public string Hall { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;
    }
}
