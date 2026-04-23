using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class FinalGrade : BaseEntities<int>
    {
        public string StudentId { get; set; } = string.Empty;
        public User? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        /// <summary>Instructor-entered final exam score, 0–60.</summary>
        public int FinalScore { get; set; } = 0;

        /// <summary>Optional bonus added on top of coursework, 0–10.</summary>
        public int Bonus { get; set; } = 0;

        public bool Published { get; set; } = false;
    }
}
