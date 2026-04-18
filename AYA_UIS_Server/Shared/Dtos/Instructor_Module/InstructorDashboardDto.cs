namespace Shared.Dtos.Instructor_Module
{
    public class InstructorCourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Students { get; set; }
        public int Progress { get; set; }
    }

    public class GradeSummaryDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public string Avg { get; set; } = string.Empty;
    }

    public class ActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Student { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    public class UpcomingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class InstructorDashboardDto
    {
        public List<InstructorCourseDto> Courses { get; set; } = new();

        /// <summary>
        /// Dictionary mapping course ID to grade summary statistics
        /// </summary>
        public Dictionary<string, GradeSummaryDto>? GradeSummary { get; set; }

        public List<ActivityDto> Activity { get; set; } = new();
        public List<UpcomingDto> Upcoming { get; set; } = new();
    }
}
