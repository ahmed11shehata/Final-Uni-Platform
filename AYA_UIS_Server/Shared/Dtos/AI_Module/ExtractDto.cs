namespace Shared.Dtos.AI_Module
{
    public class ExtractRequestDto
    {
        // Note: IFormFile is bound at controller level, not in DTO
        public string Type { get; set; } = string.Empty; // "pdf" | "docx" | "image" | "text"
    }

    public class ExtractResponseDto
    {
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
