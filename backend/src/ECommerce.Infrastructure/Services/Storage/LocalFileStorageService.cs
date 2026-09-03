using ECommerce.Application.DTOs.Media;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalFileStorageService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg", ".avif"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml", "image/avif"
    };

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<FileUploadResultDto> UploadAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        string folder = "products",
        CancellationToken cancellationToken = default)
    {
        var sanitizedFolder = SanitizeFolderName(folder);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File extension '{extension}' is not allowed for uploads.");
        }

        var uniqueFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";

        // Target directory inside web root
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var targetDirectory = Path.Combine(webRoot, "uploads", sanitizedFolder);

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var fullPath = Path.Combine(targetDirectory, uniqueFileName);
        using (var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(outputStream, cancellationToken);
        }

        var relativePath = $"/uploads/{sanitizedFolder}/{uniqueFileName}";
        _logger.LogInformation("Saved uploaded media file to {FilePath} (Relative: {RelativePath})", fullPath, relativePath);

        return new FileUploadResultDto
        {
            FileName = uniqueFileName,
            OriginalFileName = Path.GetFileName(originalFileName),
            ContentType = contentType,
            SizeBytes = fileStream.Length,
            Url = relativePath,
            RelativePath = relativePath,
            UploadedAtUtc = DateTime.UtcNow
        };
    }

    public Task<bool> DeleteAsync(string relativePathOrUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var cleanRelative = relativePathOrUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRoot, cleanRelative);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted file at {FilePath}", fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file at {Path}", relativePathOrUrl);
            return Task.FromResult(false);
        }
    }

    public bool IsValidImageFile(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension)) return false;

        return AllowedExtensions.Contains(extension) && AllowedMimeTypes.Contains(contentType);
    }

    private static string SanitizeFolderName(string folder)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(folder.Where(c => !invalid.Contains(c) && c != '.' && c != '/' && c != '\\').ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "general" : cleaned.ToLowerInvariant();
    }
}
