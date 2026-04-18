using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Auth_Module
{
    /// <summary>
    /// DTO for PUT /api/user/profile
    /// Only editable fields are accepted. Email, role, and department are READ-ONLY.
    /// </summary>
    public class UpdateProfileDto
    {
        public string? Name { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of birth must be in YYYY-MM-DD format.")]
        public string? Dob { get; set; }

        [RegularExpression("^(male|female)$", ErrorMessage = "Gender must be 'male' or 'female'.")]
        public string? Gender { get; set; }

        // READ-ONLY fields — if sent, will be rejected
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
    }
}
