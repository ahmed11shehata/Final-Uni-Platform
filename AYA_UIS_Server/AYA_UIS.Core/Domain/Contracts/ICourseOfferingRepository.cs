using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface ICourseOfferingRepository
    {
        Task<CourseOffering?> GetAsync(
            int courseId,
            int studyYearId,
            int semesterId,
            Levels level);

        Task<IEnumerable<CourseOffering>> GetAllAsync();
        Task<IEnumerable<CourseOffering>> GetByCourseIdAsync(int courseId);
        Task AddAsync(CourseOffering offering);
    }
}
