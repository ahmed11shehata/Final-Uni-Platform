using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IAdminCourseLockRepository
    {
        Task<IEnumerable<AdminCourseLock>> GetLockedCoursesForUserAsync(string userId);
        Task<AdminCourseLock?> GetAsync(string userId, int courseId);
        Task AddAsync(AdminCourseLock lockEntry);
        Task RemoveAsync(AdminCourseLock lockEntry);
        Task<bool> IsLockedAsync(string userId, int courseId);
    }
}
