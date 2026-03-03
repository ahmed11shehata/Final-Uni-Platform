using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace AYA_UIS.Core.Domain.Entities.Models
{
    public class CourseOffering : BaseEntities<int>
    {

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public int StudyYearId { get; set; }
        public int SemesterId { get; set; }

        public Levels Level { get; set; }

        public bool IsOpen { get; set; } = false;
    }
}
