namespace Shared.Dtos.Instructor_Module
{
    public class InstructorMaterialDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Downloads { get; set; }
        public string? ReleaseDate { get; set; }
    }

    public class CreateMaterialDto
    {
        // Note: IFormFile is bound at controller level with [FromForm]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
