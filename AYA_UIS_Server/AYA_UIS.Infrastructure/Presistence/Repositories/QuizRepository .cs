using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Presistence.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly UniversityDbContext _context;

        public QuizRepository(UniversityDbContext context)
        {
            _context = context;
        }

        public async Task AddQuizAsync(Quiz quiz)
        {
            await _context.Quizzes.AddAsync(quiz);
        }

        public async Task<Quiz?> GetQuizAsync(int quizId)
        {
            return await _context.Quizzes.FindAsync(quizId);
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task AddQuestionAsync(QuizQuestion question)
        {
            await _context.QuizQuestions.AddAsync(question);
        }

        public async Task AddAttemptAsync(StudentQuizAttempt attempt)
        {
            await _context.StudentQuizAttempts.AddAsync(attempt);
        }

        public async Task<bool> AttemptExists(int quizId, string studentId)
        {
            return await _context.StudentQuizAttempts
                .AnyAsync(a => a.QuizId == quizId && a.StudentId == studentId);
        }

        public async Task<IEnumerable<Quiz>>GetQuizzesByCourseId(int courseId)
        {
            return await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();
        }
    }
}
