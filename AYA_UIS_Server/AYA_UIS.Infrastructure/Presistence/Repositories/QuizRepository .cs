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
                .Where(q => q.CourseId == courseId && !q.IsArchived)
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

        public async Task<StudentQuizAttempt?> GetStudentAttemptAsync(int quizId, string studentId)
        {
            return await _dbContext.StudentQuizAttempts
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId);
        }

        public Task UpdateAsync(Quiz quiz)
        {
            _dbContext.Quizzes.Update(quiz);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Quiz quiz)
        {
            // Load and remove all owned children explicitly so we don't depend on
            // DB cascade rules being configured.
            var attempts = await _dbContext.StudentQuizAttempts
                .Where(a => a.QuizId == quiz.Id)
                .ToListAsync();
            if (attempts.Count > 0)
            {
                var attemptIds = attempts.Select(a => a.Id).ToList();
                var answers = await _dbContext.StudentAnswers
                    .Where(sa => attemptIds.Contains(sa.AttemptId))
                    .ToListAsync();
                if (answers.Count > 0) _dbContext.StudentAnswers.RemoveRange(answers);
                _dbContext.StudentQuizAttempts.RemoveRange(attempts);
            }

            var questions = await _dbContext.QuizQuestions
                .Where(q => q.QuizId == quiz.Id)
                .ToListAsync();
            if (questions.Count > 0)
            {
                var questionIds = questions.Select(q => q.Id).ToList();
                var options = await _dbContext.QuizOptions
                    .Where(o => questionIds.Contains(o.QuestionId))
                    .ToListAsync();
                if (options.Count > 0) _dbContext.QuizOptions.RemoveRange(options);
                _dbContext.QuizQuestions.RemoveRange(questions);
            }

            _dbContext.Quizzes.Remove(quiz);
        }

        public async Task<bool> HasAttemptsAsync(int quizId)
            => await _dbContext.StudentQuizAttempts.AnyAsync(a => a.QuizId == quizId);
    }
}
