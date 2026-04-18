using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.QuizDto
{
    public class CreateQuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;

        public QuestionType Type { get; set; }

        public List<CreateOptionDto> Options { get; set; }
    }
}
