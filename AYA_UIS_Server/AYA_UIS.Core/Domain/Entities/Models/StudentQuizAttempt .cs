using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Identity;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class StudentQuizAttempt : BaseEntities<int>
    {
        public int QuizId { get; set; }

        public Quiz? Quiz { get; set; }

        public string StudentId { get; set; } = string.Empty; 

        public User? Student { get; set; }

        public decimal Score { get; set; }

        public DateTime SubmittedAt { get; set; }

        public ICollection<StudentAnswer> Answers { get; set; }
            = new List<StudentAnswer>();
    }
}
