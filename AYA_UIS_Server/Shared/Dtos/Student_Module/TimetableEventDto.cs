namespace Shared.Dtos.Student_Module
{
    public class TimetableEventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Day { get; set; }
        public string? Date { get; set; }
        public decimal? Start { get; set; }
        public decimal? End { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
