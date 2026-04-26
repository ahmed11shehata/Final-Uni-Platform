using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class FinalGradeReviewRepository : IFinalGradeReviewRepository
    {
        private readonly UniversityDbContext _ctx;

        public FinalGradeReviewRepository(UniversityDbContext ctx) => _ctx = ctx;

        public async Task<FinalGradeReview?> GetAsync(string studentId, int studyYearId, int semesterId)
            => await _ctx.FinalGradeReviews
                .FirstOrDefaultAsync(r =>
                    r.StudentId == studentId &&
                    r.StudyYearId == studyYearId &&
                    r.SemesterId == semesterId);

        public async Task<List<FinalGradeReview>> GetByTermAsync(int studyYearId, int semesterId)
            => await _ctx.FinalGradeReviews
                .Where(r => r.StudyYearId == studyYearId && r.SemesterId == semesterId)
                .AsNoTracking()
                .ToListAsync();

        public async Task AddAsync(FinalGradeReview review)
            => await _ctx.FinalGradeReviews.AddAsync(review);

        public Task UpdateAsync(FinalGradeReview review)
        {
            _ctx.FinalGradeReviews.Update(review);
            return Task.CompletedTask;
        }
    }
}
