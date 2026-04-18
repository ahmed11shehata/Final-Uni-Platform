using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Admin can hard-lock a specific course for a specific student,
    /// preventing them from registering it regardless of prerequisites.
    /// </summary>
    public class AdminCourseLock : BaseEntities<int>
    {
        public string UserId   { get; set; } = string.Empty;
        public User?  User     { get; set; }
        public int    CourseId { get; set; }
        public Course? Course  { get; set; }
        public DateTime LockedAt { get; set; } = DateTime.UtcNow;
        /// <summary>Admin-written custom reason for the lock.</summary>
        public string? Reason { get; set; }
    }
}
