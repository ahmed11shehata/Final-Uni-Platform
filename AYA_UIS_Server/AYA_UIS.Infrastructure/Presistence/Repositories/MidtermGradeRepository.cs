using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class MidtermGradeRepository : IMidtermGradeRepository
    {
        private readonly UniversityDbContext _ctx;

        public MidtermGradeRepository(UniversityDbContext ctx) => _ctx = ctx;

        public async Task<MidtermGrade?> GetAsync(string studentId, int courseId)
            => await _ctx.MidtermGrades
                .FirstOrDefaultAsync(m => m.StudentId == studentId && m.CourseId == courseId);

        public async Task AddAsync(MidtermGrade grade)
            => await _ctx.MidtermGrades.AddAsync(grade);

        public Task UpdateAsync(MidtermGrade grade)
        {
            _ctx.MidtermGrades.Update(grade);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<MidtermGrade>> GetByCourseAsync(int courseId)
            => await _ctx.MidtermGrades
                .Where(m => m.CourseId == courseId)
                .ToListAsync();
    }
}
