using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Auth_Module
{
    public class ChangePasswordDto
    {
        [Required]
        [JsonPropertyName("currentPassword")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
