using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.QuizDto
{
    public class CreateOptionDto
    {
        public string Text { get; set; }

        public bool IsCorrect { get; set; }
    }
}
