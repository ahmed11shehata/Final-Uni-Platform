namespace Shared.Dtos.Instructor_Module
{
    public class StudentInCourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int Submissions { get; set; }
        public decimal? AvgGrade { get; set; }
    }
}
