namespace ECommerce.Application.DTOs.Media;

public record FileUploadResultDto
{
    public string FileName { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string Url { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public DateTime UploadedAtUtc { get; init; } = DateTime.UtcNow;
}
