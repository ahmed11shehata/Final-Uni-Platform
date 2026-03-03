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
    public class CourseResultRepository : ICourseResultRepository
    {
        private readonly UniversityDbContext _context;

        public CourseResultRepository(UniversityDbContext context)
        {
            _context = context;
        }

        public async Task<CourseResult?> GetAsync(
            string userId,
            int courseId,
            int studyYearId)
        {
            return await _context.CourseResults
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.CourseId == courseId &&
                    x.StudyYearId == studyYearId);
        }

        public async Task<List<CourseResult>> GetByUserAsync(string userId)
        {
            return await _context.CourseResults
                .Include(x => x.Course)
                .Include(x => x.StudyYear)
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(CourseResult result)
        {
            await _context.CourseResults.AddAsync(result);
        }
    }
}
