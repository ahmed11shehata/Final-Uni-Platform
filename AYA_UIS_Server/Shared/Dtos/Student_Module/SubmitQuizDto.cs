namespace Shared.Dtos.Student_Module
{
    public class SubmitQuizDto
    {
        public List<int> Answers { get; set; } = new();
    }

    public class QuizSubmitResponseDto
    {
        public int Score { get; set; }
        public int Max { get; set; }
        public bool Submitted { get; set; }
        public bool Graded { get; set; }
    }
}
