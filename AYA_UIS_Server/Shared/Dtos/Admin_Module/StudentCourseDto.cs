namespace Shared.Dtos.Admin_Module
{
    public class StudentCourseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Grade { get; set; }
        public int Credits { get; set; }
    }
}
