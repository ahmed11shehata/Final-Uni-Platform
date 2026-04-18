namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Stores the last-published JSON snapshot for a given year + schedule type.
    /// Admin edits live Sessions/Exams freely; students only see the published snapshot.
    /// </summary>
    public class SchedulePublish : BaseEntities<int>
    {
        public int Year { get; set; }                       // 1-4
        public string Type { get; set; } = "weekly";        // weekly | midterm | final
        public string PublishedData { get; set; } = "[]";   // JSON blob
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public string PublishedBy { get; set; } = string.Empty;
    }
}
