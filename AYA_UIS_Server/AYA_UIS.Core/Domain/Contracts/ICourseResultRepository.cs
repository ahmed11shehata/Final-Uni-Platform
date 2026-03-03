using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface ICourseResultRepository
    {
        Task<CourseResult?> GetAsync(
            string userId,
            int courseId,
            int studyYearId
        );

        Task<List<CourseResult>> GetByUserAsync(string userId);

        Task AddAsync(CourseResult result);
    }
}
