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
                .Include(x => x.Course)
                .Include(x => x.CreatedBy)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseIdAsync(int courseId)
            => await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.CreatedBy)
                .Include(a => a.Submissions)
                .Where(a => a.CourseId == courseId && !a.IsArchived)
                .AsNoTracking()
                .ToListAsync();

        public async Task<bool> SubmissionExists(int assignmentId, string studentId)
            => await _context.AssignmentSubmissions
                .AnyAsync(x => x.AssignmentId == assignmentId && x.StudentId == studentId);

        public async Task AddSubmissionAsync(AssignmentSubmission submission)
            => await _context.AssignmentSubmissions.AddAsync(submission);

        public async Task<IEnumerable<AssignmentSubmission>> GetSubmissions(int assignmentId)
            => await _context.AssignmentSubmissions
                .Where(x => x.AssignmentId == assignmentId && x.Status != "Cleared")
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
                .Where(a => a.CourseId == courseId && !a.IsArchived)
                .AsNoTracking()
                .ToListAsync();

        public async Task<AssignmentSubmission?> GetStudentSubmissionAsync(int assignmentId, string studentId)
            => await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        public Task UpdateSubmissionAsync(AssignmentSubmission submission)
        {
            _context.AssignmentSubmissions.Update(submission);
            return Task.CompletedTask;
        }

        public async Task DeleteSubmissionAsync(int submissionId)
        {
            var sub = await _context.AssignmentSubmissions.FindAsync(submissionId);
            if (sub != null) _context.AssignmentSubmissions.Remove(sub);
        }

        public Task UpdateAsync(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Assignment assignment)
        {
            _context.Assignments.Remove(assignment);
            return Task.CompletedTask;
        }

        public async Task<bool> HasGradedSubmissionAsync(int assignmentId)
            => await _context.AssignmentSubmissions
                .AnyAsync(s => s.AssignmentId == assignmentId &&
                               s.Status != "Cleared" &&
                               s.Grade != null);
    }
}
