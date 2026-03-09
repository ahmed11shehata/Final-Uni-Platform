using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class Quiz : BaseEntities<int>
    {
        public string Title { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public ICollection<QuizQuestion> Questions { get; set; }
            = new List<QuizQuestion>();
    }
}
