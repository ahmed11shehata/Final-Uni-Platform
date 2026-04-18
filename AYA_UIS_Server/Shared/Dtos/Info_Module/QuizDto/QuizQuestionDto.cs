using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.QuizDto
{
    public class QuizQuestionDto
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public List<QuizOptionDto> Options { get; set; } = new();
    }
}
