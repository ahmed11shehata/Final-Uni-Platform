namespace Shared.Dtos.Student_Module
{
    public class CourseAssignmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Deadline { get; set; } = string.Empty;
        public int Max { get; set; }
        public int? Grade { get; set; }
        public string? ReleaseDate { get; set; }
        /// <summary>upcoming | available | submitted | graded | rejected | locked</summary>
        public string Status { get; set; } = string.Empty;
        public List<string> Types { get; set; } = new();
        /// <summary>File name of the student's own submission (not the instructor attachment).</summary>
        public string? File { get; set; }
        /// <summary>Full URL of the instructor's assignment attachment/template, if any.</summary>
        public string? AttachmentUrl { get; set; }
        public string? RejectionReason { get; set; }
        public bool CanSubmit { get; set; }
        public int? SubmissionId { get; set; }

        // ── Submission detail (for modal display) ──
        /// <summary>Filename of the student's current active submission file.</summary>
        public string? SubmissionFileName { get; set; }
        /// <summary>Full URL to download the student's own submitted file.</summary>
        public string? SubmissionFileUrl { get; set; }
        /// <summary>Human-readable submission timestamp.</summary>
        public string? SubmissionDate { get; set; }
        /// <summary>Total submission attempts used (1 = initial, increments on each resubmit).</summary>
        public int AttemptCount { get; set; }
    }
}
