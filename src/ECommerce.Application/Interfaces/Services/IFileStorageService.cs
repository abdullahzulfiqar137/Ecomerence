using ECommerce.Application.DTOs.Media;

namespace ECommerce.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<FileUploadResultDto> UploadAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        string folder = "products",
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string relativePathOrUrl,
        CancellationToken cancellationToken = default);

    bool IsValidImageFile(string fileName, string contentType);
}
