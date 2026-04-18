using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    /// <summary>
    /// Persistent many-to-many: one course can have many instructors, one instructor many courses.
    /// Assignments survive registration open/close cycles. Cascade-deleted when the instructor
    /// account is removed from the system.
    /// </summary>
    public class RegistrationCourseInstructor : BaseEntities<int>
    {
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string InstructorId { get; set; } = string.Empty;
        public User Instructor { get; set; } = null!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
