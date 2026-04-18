using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Auth_Module
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordVerifyDto
    {
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [Required, MinLength(8)]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
