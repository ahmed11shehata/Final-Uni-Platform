using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.UserDtos
{
    public class UserProfileDto
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string AcademicCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int TotalCredits { get; set; }
        public int AllowedCredits { get; set; }
        public float TotalGPA { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public Levels Level { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
    }
}