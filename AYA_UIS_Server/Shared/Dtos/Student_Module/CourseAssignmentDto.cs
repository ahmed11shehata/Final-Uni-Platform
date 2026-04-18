namespace Shared.Dtos.Student_Module
{
    public class CourseAssignmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Deadline { get; set; } = string.Empty;
        public int Max { get; set; }
        public int? Grade { get; set; }
        public string Status { get; set; } = string.Empty; // "pending" | "upcoming" | "graded" | "submitted" | "completed"
        public List<string> Types { get; set; } = new();
        public string? File { get; set; }
    }
}
