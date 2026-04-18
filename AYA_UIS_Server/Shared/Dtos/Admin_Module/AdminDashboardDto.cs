namespace Shared.Dtos.Admin_Module
{
    public class AdminDashboardDto
    {
        public AdminStatsDto Stats { get; set; } = new();
        public List<AdminActivityDto> RecentActivity { get; set; } = new();
    }

    public class AdminStatsDto
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveRegistrations { get; set; }
    }

    public class AdminActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}
