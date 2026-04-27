using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class RegistrationRepository : GenericRepository<Registration, int>, IRegistrationRepository
    {
        public RegistrationRepository(UniversityDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Registration>> GetByUserIdAsync(string userId)
        {
            return await _dbContext.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Course)
                .Include(r => r.StudyYear)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Registration>> GetByCourseIdAsync(int courseId)
        {
            // Excludes archived rows so instructor / admin "current course roster"
            // views never surface registrations that were archived by an Academic
            // Year Reset. Historical lookups should use GetByUserIdAsync instead.
            return await _dbContext.Registrations
                .Where(r => r.CourseId == courseId && !r.IsArchived)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Registration?> GetByUserAndCourseAsync(string userId, int courseId, int studyYearId)
        {
            return await _dbContext.Registrations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId && r.StudyYearId == studyYearId);
        }

        public async Task<bool> IsUserRegisteredInCourseAsync(string userId, int courseId)
        {
            return await _dbContext.Registrations
                .AnyAsync(r => r.UserId == userId && r.CourseId == courseId);
        }

        public async Task<bool> IsCourseCompletedByUserAsync(string userId, int courseId)
        {
            return await _dbContext.Registrations
                .AnyAsync(r => r.UserId == userId && r.CourseId == courseId && r.IsPassed && r.Progress == CourseProgress.Completed);
        }

        public async Task<IEnumerable<Registration>> GetByUserAndStudyYearAsync(string userId, int studyYearId)
        {
            return await _dbContext.Registrations
                .Where(r => r.UserId == userId && r.StudyYearId == studyYearId)
                .Include(r => r.Course)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Registration>> GetByUserAndStudyYearAndSemseterAsync(string userId, int studyYearId, int semesterId)
        {
            return await _dbContext.Registrations
                .Where(r => r.UserId == userId && r.StudyYearId == studyYearId && r.SemesterId == semesterId)
                .Include(r => r.Course)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Registration>> GetByUserAsync(string userId, int? studyYearId = null)
        {
            var query = _dbContext.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Course)
                .Include(r => r.StudyYear)
                .Include(r => r.Semester)
                .AsNoTracking();

            if (studyYearId.HasValue)
                query = query.Where(r => r.StudyYearId == studyYearId.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Registration>> GetAllAsync(int? courseId = null, int? studyYearId = null, int? semesterId = null, string? userId = null)
        {
            var query = _dbContext.Registrations
                .Include(r => r.Course)
                .Include(r => r.StudyYear)
                .Include(r => r.Semester)
                .Include(r => r.User)
                .AsNoTracking();

            if (courseId.HasValue)
                query = query.Where(r => r.CourseId == courseId.Value);
            if (studyYearId.HasValue)
                query = query.Where(r => r.StudyYearId == studyYearId.Value);
            if (semesterId.HasValue)
                query = query.Where(r => r.SemesterId == semesterId.Value);
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(r => r.UserId == userId);

            return await query.ToListAsync();
        }
    }
}
