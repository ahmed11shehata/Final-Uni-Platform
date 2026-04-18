using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Auth_Module
{
    /// <summary>
    /// DTO for PUT /api/user/theme
    /// </summary>
    public class UpdateThemeDto
    {
        [Required(ErrorMessage = "themeId is required.")]
        public string ThemeId { get; set; } = string.Empty;
    }
}
