namespace Shared.Dtos.AI_Module
{
    public class GenerateRequestDto
    {
        public string Type { get; set; } = string.Empty; // "flashcards" | "summary" | "quiz"
        public string Content { get; set; } = string.Empty;
        public int? Count { get; set; }
    }

    public class GeneratedContentDto
    {
        public string Type { get; set; } = string.Empty;
        public object Content { get; set; } = new();
    }

    public class FlashcardDto
    {
        public string Front { get; set; } = string.Empty;
        public string Back { get; set; } = string.Empty;
    }

    public class SummaryDto
    {
        public string Title { get; set; } = string.Empty;
        public List<SectionDto> Sections { get; set; } = new();
    }

    public class SectionDto
    {
        public string Heading { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class QuizDto
    {
        public string Title { get; set; } = string.Empty;
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public string Q { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int Answer { get; set; }
    }
}
