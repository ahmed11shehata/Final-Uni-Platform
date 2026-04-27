using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class Assignment : BaseEntities<int>
    {
        public string Title             { get; set; } = string.Empty;
        public string Description       { get; set; } = string.Empty;
        public string FileUrl           { get; set; } = string.Empty;
        public int    Points            { get; set; }
        public DateTime Deadline        { get; set; }
        public DateTime? ReleaseDate    { get; set; }
        public int    CourseId          { get; set; }
        public Course?  Course          { get; set; }
        public string CreatedByUserId   { get; set; } = string.Empty;
        public User?    CreatedBy       { get; set; }

        // Non-nullable collection — avoids ThenInclude nullability conflict
        public ICollection<AssignmentSubmission> Submissions { get; set; }
            = new List<AssignmentSubmission>();

        // ── Reset Material (admin) soft-delete fields ──────────────
        // When IsArchived is true, the row is hidden from instructor + student
        // active material lists but remains in the DB so grade history stays consistent.
        public bool      IsArchived     { get; set; }
        public DateTime? DeletedAt      { get; set; }
        public string?   DeletedById    { get; set; }
        public int?      ResetBatchId   { get; set; }
        public DateTime? FilePurgedAt   { get; set; }
        public DateTime? ContentPurgedAt{ get; set; }
    }
}
