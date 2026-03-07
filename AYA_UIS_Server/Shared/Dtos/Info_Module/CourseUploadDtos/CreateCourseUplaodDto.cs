using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.CourseUploadDtos
{
    public class CreateCourseUploadDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public UploadType Type { get; set; } // e.g., "sheet", "sheet answer", "material", etc.
        public string UploadedByUserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    }
}