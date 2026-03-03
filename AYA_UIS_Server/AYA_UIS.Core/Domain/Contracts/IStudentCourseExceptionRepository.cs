using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IStudentCourseExceptionRepository
    {
        Task<StudentCourseException?> GetAsync(
            string userId,
            int courseId,
            int studyYearId,
            int semesterId);

        Task AddAsync(StudentCourseException entity);
    }
}
