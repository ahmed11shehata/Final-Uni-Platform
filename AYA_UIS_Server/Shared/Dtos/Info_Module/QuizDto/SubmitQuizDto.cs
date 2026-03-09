using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.QuizDto
{
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }

        public List<SubmitAnswerDto> Answers { get; set; }
    }

    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }

        public int SelectedOptionId { get; set; }
    }
}
