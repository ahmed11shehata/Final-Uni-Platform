namespace Shared.Dtos.Student_Module
{
    public class CourseQuizSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        /// <summary>"yyyy-MM-dd" — for frontend today-filter</summary>
        public string StartIso { get; set; } = string.Empty;
        /// <summary>"h:mm tt" — e.g. "9:30 AM"</summary>
        public string StartTime { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int Questions { get; set; }
        public decimal Max { get; set; }
        public decimal? Score { get; set; }
        public string Status { get; set; } = string.Empty; // "available" | "upcoming" | "completed"
        public string Deadline { get; set; } = string.Empty;
        /// <summary>True when EndTime has passed — student can view answer review</summary>
        public bool ReviewAvailable { get; set; }
    }
}
