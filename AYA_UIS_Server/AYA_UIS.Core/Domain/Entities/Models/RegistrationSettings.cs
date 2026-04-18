namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class RegistrationSettings : BaseEntities<int>
    {
        public bool   IsOpen        { get; set; } = false;
        public string Semester      { get; set; } = string.Empty;
        public string AcademicYear  { get; set; } = string.Empty;
        public DateTime? StartDate  { get; set; }
        public DateTime? Deadline   { get; set; }
        // comma-separated year numbers e.g. "1,2,3"
        public string OpenYears     { get; set; } = string.Empty;
        // comma-separated course codes
        public string EnabledCourses { get; set; } = string.Empty;
        public DateTime? OpenedAt   { get; set; }
        public DateTime? ClosedAt   { get; set; }
        public int? MaxCredits      { get; set; }  // null = use GPA-based rules
        public DateTime? SchedulePublishedAt { get; set; }
        // JSON: {"1":["CS101","CS102"],"2":["CS201"],...}
        // New per-year model. Legacy OpenYears/EnabledCourses auto-derived on save.
        public string? OpenedCoursesByYear { get; set; }
    }
}
