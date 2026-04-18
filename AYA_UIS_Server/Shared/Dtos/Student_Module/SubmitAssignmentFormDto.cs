using Microsoft.AspNetCore.Http;

namespace Shared.Dtos.Student_Module
{
    /// <summary>
    /// DTO for submitting an assignment with file upload
    /// </summary>
    public class SubmitAssignmentFormDto
    {
        /// <summary>
        /// The assignment submission file
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Optional submission notes or comments
        /// </summary>
        public string? Notes { get; set; }
    }
}
