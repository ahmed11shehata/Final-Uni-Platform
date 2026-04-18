using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly UniversityDbContext _dbContext;

        public QuizRepository(UniversityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByCourseId(int courseId)
        {
            return await _dbContext.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .Where(q => q.CourseId == courseId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _dbContext.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<Quiz?> GetQuizAsync(int quizId)
        {
            return await _dbContext.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task AddAsync(Quiz quiz)
        {
            await _dbContext.Quizzes.AddAsync(quiz);
        }

        public async Task AddQuizAsync(Quiz quiz)
        {
            await _dbContext.Quizzes.AddAsync(quiz);
        }

        public async Task AddQuestionAsync(QuizQuestion question)
        {
            await _dbContext.QuizQuestions.AddAsync(question);
        }

        public async Task<bool> AttemptExists(int quizId, string studentId)
        {
            return await _dbContext.StudentQuizAttempts
                .AnyAsync(a => a.QuizId == quizId && a.StudentId == studentId);
        }

        public async Task AddAttemptAsync(StudentQuizAttempt attempt)
        {
            await _dbContext.StudentQuizAttempts.AddAsync(attempt);
        }

        public async Task<IEnumerable<StudentQuizAttempt>> GetAttemptsByQuizIdAsync(int quizId)
        {
            return await _dbContext.StudentQuizAttempts
                .Include(a => a.Student)
                .Where(a => a.QuizId == quizId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
