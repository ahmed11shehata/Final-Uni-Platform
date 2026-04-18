namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Represents a timetable session (Lecture or Section) for a specific year+group+day.
    /// Used by the admin schedule manager.
    /// </summary>
    public class ScheduleSession : BaseEntities<int>
    {
        public int Year { get; set; }           // 1-4
        public string Group { get; set; } = "A";  // A or B
        public string Day { get; set; } = string.Empty;  // Saturday..Thursday
        public double StartTime { get; set; }   // e.g. 8.0, 10.5
        public double EndTime { get; set; }     // e.g. 10.0, 12.0
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string Type { get; set; } = "Lecture"; // Lecture | Section | Lab
        public string Instructor { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
