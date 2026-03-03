using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.CourseDtos
{
    public class CourseDto
    {
        public int Id {get; set;}
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public int Credits { get; set; }

    }
}