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

    }
}
