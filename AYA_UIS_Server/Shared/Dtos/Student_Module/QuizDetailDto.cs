namespace Shared.Dtos.Student_Module
{
    public class QuizDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseColor { get; set; } = string.Empty;
        public string CourseShade { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Questions { get; set; }
        public string Deadline { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Score { get; set; }
        public int Max { get; set; }
        public List<MCQDto> Mcq { get; set; } = new();
    }

    public class MCQDto
    {
        public string Q { get; set; } = string.Empty;
        public List<string> Opts { get; set; } = new();
        public int Ans { get; set; }
    }
}
