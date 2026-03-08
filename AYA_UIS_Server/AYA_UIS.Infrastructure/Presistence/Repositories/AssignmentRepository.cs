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
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly UniversityDbContext _context;

        public AssignmentRepository(UniversityDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Assignment assignment)
        {
            await _context.Assignments.AddAsync(assignment);
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseIdAsync(int courseId)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.CreatedBy)
                .Include(a => a.Submissions)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<bool> SubmissionExists(int assignmentId, string studentId)
        {
            return await _context.AssignmentSubmissions
                .AnyAsync(x => x.AssignmentId == assignmentId && x.StudentId == studentId);
        }

        public async Task AddSubmissionAsync(AssignmentSubmission submission)
        {
            await _context.AssignmentSubmissions.AddAsync(submission);
        }

        public async Task<IEnumerable<AssignmentSubmission>> GetSubmissions(int assignmentId)
        {
            return await _context.AssignmentSubmissions
                .Where(x => x.AssignmentId == assignmentId)
                .Include(x => x.Student)
                .ToListAsync();
        }

        public async Task<AssignmentSubmission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.AssignmentSubmissions
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == submissionId);
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsWithSubmissions(int courseId)
        {
            return await _context.Assignments
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();
        }
    }
}
