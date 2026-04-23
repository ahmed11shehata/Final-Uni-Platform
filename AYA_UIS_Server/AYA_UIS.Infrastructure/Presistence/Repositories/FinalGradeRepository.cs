using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class FinalGradeRepository : IFinalGradeRepository
    {
        private readonly UniversityDbContext _ctx;

        public FinalGradeRepository(UniversityDbContext ctx) => _ctx = ctx;

        public async Task<FinalGrade?> GetAsync(string studentId, int courseId)
            => await _ctx.FinalGrades
                .FirstOrDefaultAsync(f => f.StudentId == studentId && f.CourseId == courseId);

        public async Task AddAsync(FinalGrade grade)
            => await _ctx.FinalGrades.AddAsync(grade);

        public Task UpdateAsync(FinalGrade grade)
        {
            _ctx.FinalGrades.Update(grade);
            return Task.CompletedTask;
        }
    }
}
