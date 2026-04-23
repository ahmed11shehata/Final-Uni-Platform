using AYA_UIS.Core.Domain.Entities.Models;

namespace Domain.Contracts
{
    public interface IQuizRepository
    {
        Task<IEnumerable<Quiz>> GetQuizzesByCourseId(int courseId);
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
        Task<Quiz?> GetQuizAsync(int quizId);
        Task AddAsync(Quiz quiz);
        Task AddQuizAsync(Quiz quiz);
        Task AddQuestionAsync(QuizQuestion question);
        Task<bool> AttemptExists(int quizId, string studentId);
        Task AddAttemptAsync(StudentQuizAttempt attempt);
        Task<IEnumerable<StudentQuizAttempt>> GetAttemptsByQuizIdAsync(int quizId);
        Task<StudentQuizAttempt?> GetStudentAttemptAsync(int quizId, string studentId);
    }
}
