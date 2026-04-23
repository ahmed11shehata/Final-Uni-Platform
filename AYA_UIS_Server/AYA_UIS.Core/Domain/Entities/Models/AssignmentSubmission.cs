using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class AssignmentSubmission : BaseEntities<int>
    {
        public int AssignmentId { get; set; }

        public Assignment? Assignment { get; set; } 

        public string StudentId { get; set; } = string.Empty;

        public User? Student { get; set; } 

        public string FileUrl { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } 

        public int? Grade { get; set; }

        public string? Feedback { get; set; }

        /// <summary>Pending | Accepted | Rejected | Cleared</summary>
        public string Status { get; set; } = "Pending";

        public string? RejectionReason { get; set; }

        /// <summary>
        /// Total number of times this student has submitted (including the initial).
        /// Preserved across soft-clears so the resubmission limit can be enforced.
        /// Max = 4.
        /// </summary>
        public int AttemptCount { get; set; } = 1;
    }
}