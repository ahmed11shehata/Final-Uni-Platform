namespace Shared.Dtos.Info_Module.QuizDto
{
    /// <summary>
    /// Full quiz shape expected by uni-learn frontend for both
    /// the quiz list (QuizzesPage) and the quiz detail (QuizDetail).
    /// </summary>
    public class FrontendQuizDto
    {
        public int      Id            { get; set; }
        public string   Title         { get; set; } = string.Empty;
        public int      CourseId      { get; set; }
        public string   CourseCode    { get; set; } = string.Empty;
        public DateTime StartTime     { get; set; }
        public DateTime EndTime       { get; set; }
        public int      Duration      { get; set; }   // minutes — EndTime - StartTime
        public int      QuestionCount { get; set; }

        // Populated only in GetQuiz (detail endpoint)
        public List<FrontendQuizQuestionDto>? Questions { get; set; }
    }

    public class FrontendQuizQuestionDto
    {
        public int    Id      { get; set; }
        public string Text    { get; set; } = string.Empty;
        public string Type    { get; set; } = "MultipleChoice";
        public List<FrontendQuizOptionDto> Options { get; set; } = new();
    }

    public class FrontendQuizOptionDto
    {
        public int    Id   { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
