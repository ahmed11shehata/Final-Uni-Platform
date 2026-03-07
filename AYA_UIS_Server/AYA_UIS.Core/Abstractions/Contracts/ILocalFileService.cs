using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Abstraction.Contracts
{
    public interface ILocalFileService
    {
        Task<string> UploadCourseFileAsync(
        IFormFile file,
        string fileId,
        string courseName,
        CancellationToken cancellationToken);
    }
}
