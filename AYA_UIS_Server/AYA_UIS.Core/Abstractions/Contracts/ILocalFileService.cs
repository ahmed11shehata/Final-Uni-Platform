using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Abstraction.Contracts
{
    public interface ILocalFileService
    {
        Task<string> UploadCourseFileAsync(
        IFormFile file,
        string fileId,
        string courseName,
        UploadType type,
        CancellationToken cancellationToken);

        Task<string> UploadAssignmentFileAsync(
         IFormFile file,
         string fileId,
         int courseId,
         CancellationToken cancellationToken);

        Task<string> UploadSubmissionFileAsync(
         IFormFile file,
         string fileId,
         int assignmentId,
         CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a physical file previously returned as a public URL by one of the upload methods above.
        /// Returns silently when the URL is null/empty, points outside wwwroot, or the file is already gone.
        /// </summary>
        Task DeleteFileByUrlAsync(string? publicUrl);
    }
}
