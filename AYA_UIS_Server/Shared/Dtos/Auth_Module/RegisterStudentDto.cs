using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Auth_Module
{
    public class RegisterStudentDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Academic_Code { get; set; } = string.Empty;
        public Levels Level { get; set; } = Levels.First_Year;
        public Gender Gender { get; set; }
    }
}
