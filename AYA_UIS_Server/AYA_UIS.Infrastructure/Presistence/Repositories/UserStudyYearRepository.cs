using System.Linq.Expressions;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class UserStudyYearRepository : GenericRepository<UserStudyYear, int>, IUserStudyYearRepository
    {
        public UserStudyYearRepository(UniversityDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<UserStudyYear>> GetByUserIdAsync(string userId)
        {
            return await _dbContext.UserStudyYears
                .Include(usy => usy.StudyYear)
                .Where(usy => usy.UserId == userId)
                .OrderBy(usy => usy.StudyYear.StartYear)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<UserStudyYear>> GetStudyYearsByUserIdAsync(string userId)
        {
            // get user study year and study year details for a specific user
            return await _dbContext.UserStudyYears
                .Where(usy => usy.UserId == userId)
                .Include(usy => usy.StudyYear)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<UserStudyYear> userStudyYears)
        {
            await _dbContext.UserStudyYears.AddRangeAsync(userStudyYears);
        }

        public async Task<UserStudyYear?> GetCurrentByUserIdAsync(string userId)
        {
            return await _dbContext.UserStudyYears
                .Include(usy => usy.StudyYear)
                .Where(usy => usy.UserId == userId && usy.StudyYear.IsCurrent)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<UserStudyYear?> GetByUserAndStudyYearAsync(string userId, int studyYearId)
        {
            return await _dbContext.UserStudyYears
                .Include(usy => usy.StudyYear)
                .Where(usy => usy.UserId == userId && usy.StudyYearId == studyYearId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserStudyYear>> GetByStudyYearIdAsync(int studyYearId)
        {
            return await _dbContext.UserStudyYears
                .Include(usy => usy.StudyYear)
                .Where(usy => usy.StudyYearId == studyYearId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserStudyYear, bool>> predicate)
        {
            return await _dbContext.UserStudyYears.AnyAsync(predicate);
        }

        public async Task<List<UserStudyYear>> GetWhereAsync(Expression<Func<UserStudyYear, bool>> predicate)
        {
            return await _dbContext.UserStudyYears
                .Where(predicate)
                .ToListAsync();
        }
    }
}
