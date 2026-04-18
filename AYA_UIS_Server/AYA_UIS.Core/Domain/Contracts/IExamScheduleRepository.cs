using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IExamScheduleRepository
    {
        Task<IEnumerable<ExamScheduleEntry>> GetAllAsync();
        Task<ExamScheduleEntry?> GetByIdAsync(int id);
        Task<IEnumerable<ExamScheduleEntry>> GetByFiltersAsync(int? year = null);
        Task AddAsync(ExamScheduleEntry exam);
        Task RemoveAsync(ExamScheduleEntry exam);
    }
}
