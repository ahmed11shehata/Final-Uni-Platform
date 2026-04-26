using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IFinalGradeReviewRepository
    {
        Task<FinalGradeReview?> GetAsync(string studentId, int studyYearId, int semesterId);
        Task<List<FinalGradeReview>> GetByTermAsync(int studyYearId, int semesterId);
        Task AddAsync(FinalGradeReview review);
        Task UpdateAsync(FinalGradeReview review);
    }
}
