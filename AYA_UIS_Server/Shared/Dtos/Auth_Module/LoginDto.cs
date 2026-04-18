using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.Auth_Module
{
    public record LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(student|instructor|admin)$", ErrorMessage = "Role must be 'student', 'instructor', or 'admin'.")]
        public string Role { get; set; } = string.Empty;
    }
}
