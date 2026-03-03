using AYA_UIS.Core.Domain.Entities.Models;

namespace Domain.Contracts
{
    public interface ICourseRepository : IGenericRepository<Course, int>
    {
        Task<Course?> GetCourseUplaodsAsync(int id);
        Task<IEnumerable<Course>> GetDepartmentCoursesAsync(int departmentId);
        Task<IEnumerable<Course>> GetCourseDependenciesAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursePrerequisitesAsync(int courseId);
        Task<IEnumerable<Course>> GetPassedCoursesByUserAsync(string userId);
        Task<List<Course>> GetByCodesAsync(IEnumerable<string> codes);
        Task AddPrerequisiteAsync(CoursePrerequisite prerequisite);
        Task<IEnumerable<Course>> GetOpenCoursesAsync();
    }
}
