using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;
using Microsoft.VisualBasic.FileIO;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class QuizQuestion : BaseEntities<int>
    {
        public string QuestionText { get; set; }

        public QuestionType Type { get; set; }

        public int QuizId { get; set; }

        public Quiz Quiz { get; set; }

        public ICollection<QuizOption> Options { get; set; }
            = new List<QuizOption>();
    }
}
