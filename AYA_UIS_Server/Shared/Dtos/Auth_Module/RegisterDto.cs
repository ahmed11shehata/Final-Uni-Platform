using System.ComponentModel.DataAnnotations;
using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Auth_Module
{
    public record RegisterDto
    {
        [EmailAddress][Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Phone]
        public string? PhoneNumber { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Academic_Code { get; set; } = string.Empty;
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
    }
}
