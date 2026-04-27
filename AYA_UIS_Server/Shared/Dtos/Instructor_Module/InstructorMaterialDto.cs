namespace Shared.Dtos.Instructor_Module
{
    public class InstructorMaterialDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Downloads { get; set; }
        public string? ReleaseDate { get; set; }
        public int? Week { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        /// <summary>File name extracted from Url, for card display.</summary>
        public string? FileName { get; set; }
    }

    public class CreateMaterialDto
    {
        // Note: IFormFile is bound at controller level with [FromForm]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Multipart payload for PUT /api/instructor/courses/{courseId}/materials/{id}.
    /// Any non-null field is applied; new file replaces the old one (and deletes the
    /// old physical file from disk).
    /// </summary>
    public class UpdateMaterialDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
    }
}
