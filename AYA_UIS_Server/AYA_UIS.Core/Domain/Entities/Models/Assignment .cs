using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class Assignment : BaseEntities<int>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string FileUrl { get; set; }

        public int Points { get; set; }

        public DateTime Deadline { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public string CreatedByUserId { get; set; }

        public User CreatedBy { get; set; }

        public ICollection<AssignmentSubmission> Submissions { get; set; }
    }
}
