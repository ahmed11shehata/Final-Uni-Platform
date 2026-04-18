using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IRegistrationCourseInstructorRepository
    {
        /// <summary>All assignments for the given course IDs (loads Instructor nav).</summary>
        Task<List<RegistrationCourseInstructor>> GetByCourseIdsAsync(IEnumerable<int> courseIds);

        /// <summary>All assignments for a single course (for replace-all operations).</summary>
        Task<List<RegistrationCourseInstructor>> GetByCourseAsync(int courseId);

        Task AddRangeAsync(IEnumerable<RegistrationCourseInstructor> entities);
        Task RemoveRangeAsync(IEnumerable<RegistrationCourseInstructor> entities);
    }
}
