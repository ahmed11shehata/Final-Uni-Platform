using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class CourseRepository : GenericRepository<Course, int>, ICourseRepository
    {
        public CourseRepository(UniversityDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<Course?> GetCourseUplaodsAsync(int id)
        {
            return await _dbContext.Courses
                .Include(c => c.CourseUpload)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<IEnumerable<Course>> GetDepartmentCoursesAsync(int departmentId)
        {
            return _dbContext.Courses
                .Where(c => c.DepartmentId == departmentId)
                .AsNoTracking()
                .ToListAsync()
                .ContinueWith(t => t.Result.AsEnumerable());
        }

        public Task<IEnumerable<Course>> GetPassedCoursesByUserAsync(string userId)
        {
            return _dbContext.Registrations
                .Where(r => r.UserId == userId && r.IsPassed)
                .Include(r => r.Course)
                .AsNoTracking()
                .Select(r => r.Course)
                .ToListAsync()
                .ContinueWith(t => t.Result.AsEnumerable());
        }

        public async Task<IEnumerable<Course>> GetCoursePrerequisitesAsync(int courseId)
        {
            return await _dbContext.CoursePrerequisites
                .Where(cp => cp.CourseId == courseId)
                .Include(cp => cp.PrerequisiteCourse)
                .Select(cp => cp.PrerequisiteCourse)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCourseDependenciesAsync(int courseId)
        {
            return await _dbContext.CoursePrerequisites
                .Where(cp => cp.PrerequisiteCourseId == courseId)  // Fixed: Use PrerequisiteCourseId instead of RequiredCourseId
                .Include(cp => cp.Course)
                .Select(cp => cp.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetOpenCoursesAsync()
        {
            return await _dbContext.Courses
                .Where(c => c.Status == CourseStatus.Opened)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Course>> GetByCodesAsync(IEnumerable<string> codes)
        {
            return await _dbContext.Courses
                .Where(c => codes.Contains(c.Code))
                .ToListAsync();
        }

        public async Task AddPrerequisiteAsync(CoursePrerequisite prerequisite)
        {
            await _dbContext.CoursePrerequisites.AddAsync(prerequisite);
        }
    }
}
