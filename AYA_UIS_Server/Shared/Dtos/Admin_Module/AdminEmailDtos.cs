using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Dtos.Admin_Module
{
    // ── GET /api/admin/emails ──────────────────────────────────
    public class AdminEmailListResponseDto
    {
        [JsonPropertyName("accounts")]
        public List<AdminAccountDto> Accounts { get; set; } = new();

        [JsonPropertyName("counts")]
        public AdminEmailCountsDto Counts { get; set; } = new();
    }

    public class AdminAccountDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = "••••••••••••••";

        [JsonPropertyName("subEmail")]
        public string SubEmail { get; set; } = string.Empty;
    }

    public class AdminEmailCountsDto
    {
        [JsonPropertyName("student")]
        public int Student { get; set; }

        [JsonPropertyName("instructor")]
        public int Instructor { get; set; }

        [JsonPropertyName("admin")]
        public int Admin { get; set; }

        [JsonPropertyName("suspended")]
        public int Suspended { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    // ── POST /api/admin/emails/create ──────────────────────────
    public class CreateEmailAccountDto
    {
        [Required]
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(student|instructor|admin)$",
            ErrorMessage = "Role must be 'student', 'instructor', or 'admin'.")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("subEmail")]
        public string SubEmail { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        /// <summary>Optional — if omitted the server auto-generates a secure password.</summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }

    public class CreateEmailAccountResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("active")]
        public bool Active { get; set; } = true;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    // ── PUT /api/admin/emails/:id/reset-password ───────────────
    public class AdminResetPasswordBodyDto
    {
        [Required, MinLength(8)]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    // ── PUT /api/admin/emails/:id ──────────────────────────────
    public class UpdateAccountDto
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("subEmail")]
        public string? SubEmail { get; set; }
    }
}
