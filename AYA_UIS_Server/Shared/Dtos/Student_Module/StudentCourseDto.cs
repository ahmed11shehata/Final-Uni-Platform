namespace Shared.Dtos.Student_Module
{
    /// <summary>
    /// Matches frontend CoursesPage enrolled course card shape.
    /// </summary>
    public class StudentCourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Credits { get; set; }
        public int Progress { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Shade { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Students { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
