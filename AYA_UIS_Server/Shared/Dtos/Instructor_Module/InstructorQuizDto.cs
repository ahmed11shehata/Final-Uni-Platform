namespace Shared.Dtos.Instructor_Module
{
    public class InstructorQuizDto
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Questions { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Deadline { get; set; } = string.Empty;
        public int Submissions { get; set; }
        public decimal? AvgScore { get; set; }
    }

    public class CreateInstructorQuizDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public List<QuizQuestionInputDto> Questions { get; set; } = new();
        public int GradePerQ { get; set; }
        public string Deadline { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class QuizQuestionInputDto
    {
        public string Text { get; set; } = string.Empty;
        public List<AnswerOptionDto> Answers { get; set; } = new();
        public int Correct { get; set; }
    }

    public class AnswerOptionDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class QuizSubmissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubmittedAt { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Max { get; set; }
        public List<int> Answers { get; set; } = new();
    }
}
