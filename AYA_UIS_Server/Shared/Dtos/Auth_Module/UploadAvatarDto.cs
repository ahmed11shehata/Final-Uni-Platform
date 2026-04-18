using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Shared.Dtos.Auth_Module
{
    /// <summary>
    /// DTO for POST /api/user/avatar
    /// multipart/form-data with a single "file" field for avatar image upload.
    /// </summary>
    public class UploadAvatarDto
    {
        [Required(ErrorMessage = "File is required.")]
        public IFormFile File { get; set; }
    }
}
