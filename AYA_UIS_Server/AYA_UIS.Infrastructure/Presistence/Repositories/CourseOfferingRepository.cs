using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Presistence.Repositories
{
    public class CourseOfferingRepository : ICourseOfferingRepository
    {
        private readonly UniversityDbContext _context;

        public CourseOfferingRepository(UniversityDbContext context)
        {
            _context = context;
        }

        public async Task<CourseOffering?> GetAsync(
            int courseId,
            int studyYearId,
            int semesterId,
            Levels level)
        {
            return await _context.CourseOfferings
                .FirstOrDefaultAsync(x =>
                    x.CourseId == courseId &&
                    x.StudyYearId == studyYearId &&
                    x.SemesterId == semesterId &&
                    x.Level == level);
        }

        public async Task AddAsync(CourseOffering offering)
        {
            await _context.CourseOfferings.AddAsync(offering);
        }
    }
}
