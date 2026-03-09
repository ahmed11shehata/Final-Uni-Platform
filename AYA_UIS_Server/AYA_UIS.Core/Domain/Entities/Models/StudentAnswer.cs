namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class StudentAnswer : BaseEntities<int>
    {
        public int QuestionId { get; set; }

        public QuizQuestion Question { get; set; }

        public int SelectedOptionId { get; set; }

        public QuizOption SelectedOption { get; set; }

        public int AttemptId { get; set; }

        public StudentQuizAttempt Attempt { get; set; }
    }
}