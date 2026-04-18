using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class ExamScheduleRepository : IExamScheduleRepository
    {
        private readonly UniversityDbContext _ctx;

        public ExamScheduleRepository(UniversityDbContext ctx)
            => _ctx = ctx;

        public async Task<IEnumerable<ExamScheduleEntry>> GetAllAsync()
            => await _ctx.ExamScheduleEntries
                .Include(e => e.Course)
                .AsNoTracking()
                .ToListAsync();

        public async Task<ExamScheduleEntry?> GetByIdAsync(int id)
            => await _ctx.ExamScheduleEntries
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<IEnumerable<ExamScheduleEntry>> GetByFiltersAsync(int? year = null)
        {
            var query = _ctx.ExamScheduleEntries
                .Include(e => e.Course)
                .AsQueryable();

            if (year.HasValue)
                query = query.Where(e => e.Year == year.Value);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(ExamScheduleEntry exam)
        {
            ArgumentNullException.ThrowIfNull(exam);
            await _ctx.ExamScheduleEntries.AddAsync(exam);
        }

        public Task RemoveAsync(ExamScheduleEntry exam)
        {
            ArgumentNullException.ThrowIfNull(exam);
            _ctx.ExamScheduleEntries.Remove(exam);
            return Task.CompletedTask;
        }
    }
}
