using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.UserDtos
{
    public class userProfileDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AcademicCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int? TotalCredits { get; set; }
        public int? AllowedCredits { get; set; }
        public decimal? TotalGPA { get; set; }
        public string? Specialization { get; set; }
        public Levels? Level { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
    }
}