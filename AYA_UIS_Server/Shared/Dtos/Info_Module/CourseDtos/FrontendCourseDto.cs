namespace Shared.Dtos.Info_Module.CourseDtos
{
    /// <summary>
    /// Course shape expected by the uni-learn React frontend.
    /// Includes year/semester info extracted from the course Code prefix,
    /// plus display helpers (color, type, prereqs).
    /// </summary>
    public class FrontendCourseDto
    {
        public int     Id          { get; set; }
        public string  Code        { get; set; } = string.Empty;
        public string  Name        { get; set; } = string.Empty;
        public int     Credits     { get; set; }
        public int     Year        { get; set; }   // 1–4 (derived from Code or CourseOffering)
        public int     Semester    { get; set; }   // 1 or 2
        public string  Type        { get; set; } = "mandatory";   // mandatory | elective
        public string  Status      { get; set; } = "Closed";      // Opened | Closed
        public string  Dept        { get; set; } = string.Empty;
        public string[]  Prereqs   { get; set; } = Array.Empty<string>();
        public string  Color       { get; set; } = "#818cf8";
        public string? Instructor  { get; set; }
        public string? Description { get; set; }

        // For registration page (added by the endpoint when context is available)
        public string  RegStatus   { get; set; } = "available"; // registered|available|locked|completed|failed
        public decimal? Grade      { get; set; }
        public bool    IsPassed    { get; set; }
    }
}
