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
        public int    CourseId          { get; set; }
        public Course?  Course          { get; set; }
        public string CreatedByUserId   { get; set; } = string.Empty;
        public User?    CreatedBy       { get; set; }

        // Non-nullable collection — avoids ThenInclude nullability conflict
        public ICollection<AssignmentSubmission> Submissions { get; set; }
            = new List<AssignmentSubmission>();
    }
}
