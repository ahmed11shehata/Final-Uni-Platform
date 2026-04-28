using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IMidtermGradeRepository
    {
        Task<MidtermGrade?> GetAsync(string studentId, int courseId);
        Task AddAsync(MidtermGrade grade);
        Task UpdateAsync(MidtermGrade grade);
        /// <summary>
        /// Returns every midterm row (one per student) for the given course.
        /// Used by midterm-Max normalization so the course has one consistent cap.
        /// </summary>
        Task<IReadOnlyList<MidtermGrade>> GetByCourseAsync(int courseId);
    }
}
