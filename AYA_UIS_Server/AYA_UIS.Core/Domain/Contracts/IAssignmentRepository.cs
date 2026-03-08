using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IAssignmentRepository
    {
        Task<Assignment?> GetByIdAsync(int id);

        Task<IEnumerable<Assignment>> GetAssignmentsByCourseIdAsync(int courseId);

        Task AddAsync(Assignment assignment);

        Task<bool> SubmissionExists(int assignmentId, string studentId);

        Task AddSubmissionAsync(AssignmentSubmission submission);

        Task<IEnumerable<AssignmentSubmission>> GetSubmissions(int assignmentId);

        Task<AssignmentSubmission?> GetSubmissionByIdAsync(int submissionId);

        Task<IEnumerable<Assignment>> GetAssignmentsWithSubmissions(int courseId);
    }
}
