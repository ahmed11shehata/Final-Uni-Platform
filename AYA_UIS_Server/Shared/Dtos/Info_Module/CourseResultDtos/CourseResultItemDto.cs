using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.CourseResultDtos
{
    public class CourseResultItemDto
    {
        public int CourseId { get; set; }
        public bool IsPassed { get; set; }
        public decimal Grade { get; set; }
    }
}
