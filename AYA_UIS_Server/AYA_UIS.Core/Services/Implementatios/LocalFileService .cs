using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Abstraction.Contracts;
using Microsoft.Extensions.Configuration;

public class LocalFileService : ILocalFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public LocalFileService(IWebHostEnvironment env, IConfiguration configuration)
    {
        _env = env;
        _configuration = configuration;

    }

    public async Task<string> UploadCourseFileAsync(
        IFormFile file,
        string fileId,
        string courseName,
        CancellationToken cancellationToken)
    {
        var safeCourseName = courseName.Replace(" ", "_").ToLower();

        var folderPath = Path.Combine(
            _env.WebRootPath,
            "upload-course",
            safeCourseName
        );

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"course_{fileId}{Path.GetExtension(file.FileName)}";

        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream, cancellationToken);
        var baseUrl = _configuration["URLS:BaseUrl"];

        return $"{baseUrl}/upload-course/{safeCourseName}/{fileName}";
    }
}