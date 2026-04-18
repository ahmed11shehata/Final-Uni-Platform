using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class ScheduleSessionRepository : IScheduleSessionRepository
    {
        private readonly UniversityDbContext _ctx;

        public ScheduleSessionRepository(UniversityDbContext ctx)
            => _ctx = ctx;

        public async Task<IEnumerable<ScheduleSession>> GetAllAsync()
            => await _ctx.ScheduleSessions
                .Include(s => s.Course)
                .AsNoTracking()
                .ToListAsync();

        public async Task<ScheduleSession?> GetByIdAsync(int id)
            => await _ctx.ScheduleSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<ScheduleSession>> GetByFiltersAsync(
            int? year = null, string? group = null)
        {
            var query = _ctx.ScheduleSessions
                .Include(s => s.Course)
                .AsQueryable();

            if (year.HasValue)
                query = query.Where(s => s.Year == year.Value);
            if (!string.IsNullOrEmpty(group))
                query = query.Where(s => s.Group == group);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<bool> HasOverlapAsync(
            int year, string group, string day,
            double startTime, double endTime, int? excludeId = null)
        {
            var query = _ctx.ScheduleSessions.Where(s =>
                s.Year == year &&
                s.Group == group &&
                s.Day == day &&
                s.StartTime < endTime &&
                s.EndTime > startTime);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task AddAsync(ScheduleSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            await _ctx.ScheduleSessions.AddAsync(session);
        }

        public Task RemoveAsync(ScheduleSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            _ctx.ScheduleSessions.Remove(session);
            return Task.CompletedTask;
        }
    }
}
