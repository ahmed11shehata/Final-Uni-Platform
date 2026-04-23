using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IMidtermGradeRepository
    {
        Task<MidtermGrade?> GetAsync(string studentId, int courseId);
        Task AddAsync(MidtermGrade grade);
        Task UpdateAsync(MidtermGrade grade);
    }
}
