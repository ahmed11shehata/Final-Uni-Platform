using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.QuizDto
{
    public class QuizDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
