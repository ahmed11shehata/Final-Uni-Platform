namespace Shared.Dtos.Instructor_Module
{
    public class InstructorAssignmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Deadline { get; set; } = string.Empty;
        public string? ReleaseDate { get; set; }
        public int MaxGrade { get; set; }
        public List<string> AllowedFormats { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public int SubmissionsCount { get; set; }
        public int PendingCount { get; set; }
        public string Description { get; set; } = string.Empty;
        /// <summary>Full URL of the instructor's uploaded attachment/starter file, if any.</summary>
        public string? AttachmentUrl { get; set; }
    }

    public class CreateInstructorAssignmentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string Deadline { get; set; } = string.Empty;
        public string? ReleaseDate { get; set; }
        public int MaxGrade { get; set; }
        public List<string> AllowedFormats { get; set; } = new();
    }
}
