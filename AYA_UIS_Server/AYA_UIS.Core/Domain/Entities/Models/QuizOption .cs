using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class QuizOption : BaseEntities<int>
    {
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }

        public QuizQuestion? Question { get; set; } = null!;
    }
}
