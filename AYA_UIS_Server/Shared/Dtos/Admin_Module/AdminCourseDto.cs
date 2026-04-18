namespace Shared.Dtos.Admin_Module
{
    public class AdminCourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Credits { get; set; }
        public int Enrolled { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CreateAdminCourseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Credits { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Prerequisites { get; set; } = new();
        public string Semester { get; set; } = string.Empty;
    }
}
