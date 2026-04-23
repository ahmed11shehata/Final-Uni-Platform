using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IFinalGradeRepository
    {
        Task<FinalGrade?> GetAsync(string studentId, int courseId);
        Task AddAsync(FinalGrade grade);
        Task UpdateAsync(FinalGrade grade);
    }
}
