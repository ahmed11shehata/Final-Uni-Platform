using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class AdminCourseLockRepository : IAdminCourseLockRepository
    {
        private readonly UniversityDbContext _ctx;

        public AdminCourseLockRepository(UniversityDbContext ctx)
            => _ctx = ctx;

        public async Task<IEnumerable<AdminCourseLock>> GetLockedCoursesForUserAsync(
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Enumerable.Empty<AdminCourseLock>();

            return await _ctx.AdminCourseLocks
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AdminCourseLock?> GetAsync(string userId, int courseId)
        {
            if (string.IsNullOrWhiteSpace(userId) || courseId <= 0)
                return null;

            return await _ctx.AdminCourseLocks
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId);
        }

        public async Task AddAsync(AdminCourseLock lockEntry)
        {
            ArgumentNullException.ThrowIfNull(lockEntry);
            await _ctx.AdminCourseLocks.AddAsync(lockEntry);
        }

        public Task RemoveAsync(AdminCourseLock lockEntry)
        {
            ArgumentNullException.ThrowIfNull(lockEntry);
            _ctx.AdminCourseLocks.Remove(lockEntry);
            return Task.CompletedTask;
        }

        public async Task<bool> IsLockedAsync(string userId, int courseId)
        {
            if (string.IsNullOrWhiteSpace(userId) || courseId <= 0)
                return false;

            return await _ctx.AdminCourseLocks
                .AnyAsync(x => x.UserId == userId && x.CourseId == courseId);
        }
    }
}
