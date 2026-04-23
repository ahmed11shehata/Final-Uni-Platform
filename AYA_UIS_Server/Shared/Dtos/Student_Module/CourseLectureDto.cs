namespace Shared.Dtos.Student_Module
{
    public class CourseLectureDto
    {
        public string Id { get; set; } = string.Empty;
        public int? Week { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "video" | "pdf"
        public string Duration { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public bool Watched { get; set; }
        /// <summary>Full URL to the uploaded file — used for Open and Download.</summary>
        public string Url { get; set; } = string.Empty;
    }
}
