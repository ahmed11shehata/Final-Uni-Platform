using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class Quiz : BaseEntities<int>
    {
        public string Title { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int CourseId { get; set; }

        public Course? Course { get; set; }

        // Points awarded per correct answer. Quiz total = Questions.Count * GradePerQuestion.
        // Default 1 keeps backwards-compatibility for quizzes created before the column existed.
        public int GradePerQuestion { get; set; } = 1;

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        // ── Reset Material soft-delete fields ─────────────────────
        public bool      IsArchived   { get; set; }
        public DateTime? DeletedAt    { get; set; }
        public string?   DeletedById  { get; set; }
        public int?      ResetBatchId { get; set; }
        public DateTime? FilePurgedAt { get; set; }
    }
}
