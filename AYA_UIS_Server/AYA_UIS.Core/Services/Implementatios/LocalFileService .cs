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

    public async Task<string> UploadAssignmentFileAsync(
     IFormFile file,
     string fileId,
     int courseId,
     CancellationToken cancellationToken)
    {
        var folderPath = Path.Combine(
            _env.WebRootPath,
            "assignments",
            $"course_{courseId}"
        );

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"assignment_{fileId}{Path.GetExtension(file.FileName)}";

        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream, cancellationToken);

        var baseUrl = _configuration["URLS:BaseUrl"];

        return $"{baseUrl}/assignments/course_{courseId}/{fileName}";
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

    public async Task<string> UploadSubmissionFileAsync(
     IFormFile file,
     string fileId,
     int assignmentId,
     CancellationToken cancellationToken)
    {
        var folderPath = Path.Combine(
            _env.WebRootPath,
            "submissions",
            $"assignment_{assignmentId}"
        );

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"submission_{fileId}{Path.GetExtension(file.FileName)}";

        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream, cancellationToken);

        var baseUrl = _configuration["URLS:BaseUrl"];

        return $"{baseUrl}/submissions/assignment_{assignmentId}/{fileName}";
    }

    public Task DeleteFileByUrlAsync(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl)) return Task.CompletedTask;

        // Resolve the relative path under wwwroot from the stored URL.
        // We accept either a fully-qualified URL or a relative path.
        string? relative = null;
        try
        {
            if (Uri.TryCreate(publicUrl, UriKind.Absolute, out var abs))
                relative = abs.AbsolutePath; // e.g. "/assignments/course_1/foo.pdf"
            else
                relative = publicUrl.StartsWith("/") ? publicUrl : "/" + publicUrl;
        }
        catch { return Task.CompletedTask; }

        if (string.IsNullOrWhiteSpace(relative)) return Task.CompletedTask;

        // Convert "/folder/file.ext" → physical path under wwwroot
        var trimmed = relative.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, trimmed));

        // Safety: resolved path must remain inside wwwroot
        var rootFull = Path.GetFullPath(_env.WebRootPath);
        if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch { /* best-effort: never throw on cleanup */ }

        return Task.CompletedTask;
    }
}