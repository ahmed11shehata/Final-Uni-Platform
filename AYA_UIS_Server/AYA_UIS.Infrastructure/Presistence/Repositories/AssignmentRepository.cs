using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly UniversityDbContext _context;

        public AssignmentRepository(UniversityDbContext context)
            => _context = context;

        public async Task AddAsync(Assignment assignment)
            => await _context.Assignments.AddAsync(assignment);

        public async Task<Assignment?> GetByIdAsync(int id)
            => await _context.Assignments
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseIdAsync(int courseId)
            => await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.CreatedBy)
                .Include(a => a.Submissions)
                .Where(a => a.CourseId == courseId)
                .AsNoTracking()
                .ToListAsync();

        public async Task<bool> SubmissionExists(int assignmentId, string studentId)
            => await _context.AssignmentSubmissions
                .AnyAsync(x => x.AssignmentId == assignmentId && x.StudentId == studentId);

        public async Task AddSubmissionAsync(AssignmentSubmission submission)
            => await _context.AssignmentSubmissions.AddAsync(submission);

        public async Task<IEnumerable<AssignmentSubmission>> GetSubmissions(int assignmentId)
            => await _context.AssignmentSubmissions
                .Where(x => x.AssignmentId == assignmentId)
                .Include(x => x.Student)
                .AsNoTracking()
                .ToListAsync();

        public async Task<AssignmentSubmission?> GetSubmissionByIdAsync(int submissionId)
            => await _context.AssignmentSubmissions
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == submissionId);

        public async Task<IEnumerable<Assignment>> GetAssignmentsWithSubmissions(int courseId)
            => await _context.Assignments
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                .Where(a => a.CourseId == courseId)
                .AsNoTracking()
                .ToListAsync();
    }
}
