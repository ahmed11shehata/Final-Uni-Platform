namespace Shared.Dtos.Student_Module
{
    /// <summary>
    /// Normalized timetable activity (assignment, quiz, lecture material) returned by
    /// GET /api/student/timetable/events. Status / availability is computed server-side
    /// in UTC so the frontend never decides whether a link is openable.
    /// </summary>
    public class StudentTimetableActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public int SourceId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public string? ReleaseAt { get; set; }
        public string? StartAt { get; set; }
        public string? EndAt { get; set; }
        public string? DeadlineAt { get; set; }

        public string Status { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsExpired { get; set; }

        public string? ActionUrl { get; set; }
        public string? AttachmentUrl { get; set; }

        public string? LockedMessage { get; set; }
        public string? ExpiredMessage { get; set; }
    }
}
