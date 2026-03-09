using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IQuizRepository
    {
        Task AddQuizAsync(Quiz quiz);

        Task<Quiz?> GetQuizAsync(int quizId);

        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);

        Task AddQuestionAsync(QuizQuestion question);

        Task AddAttemptAsync(StudentQuizAttempt attempt);

        Task<bool> AttemptExists(int quizId, string studentId);

        Task<IEnumerable<Quiz>> GetQuizzesByCourseId(int courseId);
    }
}
