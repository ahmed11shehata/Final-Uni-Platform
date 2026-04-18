namespace Shared.Dtos.Info_Module.QuizDto
{
    public class QuizAttemptDto
    {
        public string    StudentId   { get; set; } = string.Empty;
        public string    StudentName { get; set; } = string.Empty;
        public int       Score       { get; set; }
        public DateTime  SubmittedAt { get; set; }
    }
}
