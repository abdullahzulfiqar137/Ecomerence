namespace ECommerce.Application.Interfaces.Services;

public record IdempotencyCheckResult(
    bool Exists,
    bool IsProcessing,
    int? StatusCode,
    string? ResponseBody,
    bool HashMismatch);

public interface IIdempotencyService
{
    Task<IdempotencyCheckResult> CheckAsync(
        string key,
        string path,
        string method,
        string requestHash,
        CancellationToken cancellationToken = default);

    Task RegisterProcessingAsync(
        string key,
        string path,
        string method,
        string requestHash,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    Task SaveResponseAsync(
        string key,
        int statusCode,
        string responseBody,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        string key,
        CancellationToken cancellationToken = default);
}
