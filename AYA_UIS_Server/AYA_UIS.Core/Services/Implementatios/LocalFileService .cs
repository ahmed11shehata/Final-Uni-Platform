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
using AYA_UIS.Core.Domain.Enums;

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
    UploadType type,
    CancellationToken cancellationToken)
    {
        var safeCourseName = courseName.Replace(" ", "_").ToLower();

        var folderType = type switch
        {
            UploadType.Lecture => "lectures",
            UploadType.Section => "sections",
            UploadType.Material => "materials",
            _ => "others"
        };

        var folderPath = Path.Combine(
            _env.WebRootPath,
            "upload-course",
            safeCourseName,
            folderType
        );

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"course_{fileId}{Path.GetExtension(file.FileName)}";

        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        var baseUrl = _configuration["URLS:BaseUrl"];

        return $"{baseUrl}/upload-course/{safeCourseName}/{folderType}/{fileName}";
    }
}