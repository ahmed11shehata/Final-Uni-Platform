using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Dtos.Info_Module.UserDtos
{
    public class UpdateStudentSpecializationDto
    {
        public string? AcademicCode { get; set; }
        public string Specialization { get; set; } = string.Empty;
    }
}