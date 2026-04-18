namespace Shared.Dtos.Student_Module
{
    public class SessionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public decimal Start { get; set; }
        public decimal End { get; set; }
        public string Type { get; set; } = string.Empty; // "Lecture" | "Section" | "Lab"
        public string Room { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
    }
}
