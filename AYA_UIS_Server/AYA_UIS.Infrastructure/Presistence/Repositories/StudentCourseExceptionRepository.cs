using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Presistence.Repositories
{
    public class StudentCourseExceptionRepository
     : IStudentCourseExceptionRepository
    {
        private readonly UniversityDbContext _context;

        public StudentCourseExceptionRepository(
            UniversityDbContext context)
        {
            _context = context;
        }

        public async Task<StudentCourseException?> GetAsync(
            string userId,
            int courseId,
            int studyYearId,
            int semesterId)
        {
            return await _context.StudentCourseExceptions
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.CourseId == courseId &&
                    x.StudyYearId == studyYearId &&
                    x.SemesterId == semesterId);
        }

        public async Task<IEnumerable<StudentCourseException>> GetForUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Enumerable.Empty<StudentCourseException>();

            return await _context.StudentCourseExceptions
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(StudentCourseException entity)
        {
            await _context.StudentCourseExceptions.AddAsync(entity);
        }
    }
}
