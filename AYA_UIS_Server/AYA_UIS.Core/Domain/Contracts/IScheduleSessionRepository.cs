using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IScheduleSessionRepository
    {
        Task<IEnumerable<ScheduleSession>> GetAllAsync();
        Task<ScheduleSession?> GetByIdAsync(int id);
        Task<IEnumerable<ScheduleSession>> GetByFiltersAsync(int? year = null, string? group = null);
        Task<bool> HasOverlapAsync(int year, string group, string day, double startTime, double endTime, int? excludeId = null);
        Task AddAsync(ScheduleSession session);
        Task RemoveAsync(ScheduleSession session);
    }
}
