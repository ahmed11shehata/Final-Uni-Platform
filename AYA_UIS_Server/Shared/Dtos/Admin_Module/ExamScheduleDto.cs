namespace Shared.Dtos.Admin_Module
{
    public class ExamScheduleDto
    {
        public string Id { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Hall { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class CreateExamScheduleDto
    {
        public int Year { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Hall { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
