using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class RegistrationCourseInstructorRepository : IRegistrationCourseInstructorRepository
    {
        private readonly UniversityDbContext _ctx;

        public RegistrationCourseInstructorRepository(UniversityDbContext ctx)
            => _ctx = ctx;

        public async Task<List<RegistrationCourseInstructor>> GetByCourseIdsAsync(IEnumerable<int> courseIds)
            => await _ctx.RegistrationCourseInstructors
                .Include(x => x.Instructor)
                .Where(x => courseIds.Contains(x.CourseId))
                .ToListAsync();

        public async Task<List<RegistrationCourseInstructor>> GetByCourseAsync(int courseId)
            => await _ctx.RegistrationCourseInstructors
                .Include(x => x.Instructor)
                .Where(x => x.CourseId == courseId)
                .ToListAsync();

        public async Task AddRangeAsync(IEnumerable<RegistrationCourseInstructor> entities)
            => await _ctx.RegistrationCourseInstructors.AddRangeAsync(entities);

        public Task RemoveRangeAsync(IEnumerable<RegistrationCourseInstructor> entities)
        {
            _ctx.RegistrationCourseInstructors.RemoveRange(entities);
            return Task.CompletedTask;
        }
    }
}
