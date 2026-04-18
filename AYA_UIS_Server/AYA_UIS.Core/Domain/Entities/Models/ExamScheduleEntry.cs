namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Represents an exam entry (midterm or final) in the schedule.
    /// Used by the admin schedule manager.
    /// </summary>
    public class ExamScheduleEntry : BaseEntities<int>
    {
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string Type { get; set; } = "midterm";   // midterm | final
        public DateTime Date { get; set; }
        public double StartTime { get; set; }   // e.g. 9.0
        public double Duration { get; set; }    // in hours, e.g. 2.0
        public int Year { get; set; }           // 1-4
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
