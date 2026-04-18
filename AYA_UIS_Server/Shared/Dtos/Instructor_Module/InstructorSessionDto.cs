namespace Shared.Dtos.Instructor_Module
{
    public class InstructorSessionDto
    {
        public string Id { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public decimal Start { get; set; }
        public decimal End { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
