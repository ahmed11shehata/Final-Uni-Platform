namespace Shared.Dtos.Student_Module
{
    public class CourseQuizSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int Questions { get; set; }
        public int Max { get; set; }
        public int? Score { get; set; }
        public string Status { get; set; } = string.Empty; // "available" | "upcoming" | "completed"
        public string Deadline { get; set; } = string.Empty;
    }
}
