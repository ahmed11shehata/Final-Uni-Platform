using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class MidtermGrade : BaseEntities<int>
    {
        public string StudentId { get; set; } = string.Empty;
        public User? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public int Grade { get; set; }
        public int Max { get; set; }

        public bool Published { get; set; }

        public int? StudyYearId { get; set; }
        public StudyYear? StudyYear { get; set; }
    }
}
