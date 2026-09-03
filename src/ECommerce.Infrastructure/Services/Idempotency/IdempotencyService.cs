using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Idempotency;

public class IdempotencyService : IIdempotencyService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<IdempotencyService> _logger;

    private const string CachePrefix = "idempotency:";

    public IdempotencyService(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<IdempotencyService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IdempotencyCheckResult> CheckAsync(
        string key,
        string path,
        string method,
        string requestHash,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CachePrefix}{key}";
        var cached = await _cacheService.GetAsync<IdempotentRequest>(cacheKey, cancellationToken);
        if (cached != null)
        {
            if (cached.RequestHash != requestHash)
            {
                return new IdempotencyCheckResult(true, false, null, null, HashMismatch: true);
            }

            return new IdempotencyCheckResult(true, cached.IsProcessing, cached.ResponseStatusCode, cached.ResponseBody, false);
        }

        var record = await _context.IdempotentRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key && x.ExpiresAtUtc > DateTime.UtcNow, cancellationToken);

        if (record == null)
        {
            return new IdempotencyCheckResult(false, false, null, null, false);
        }

        if (record.RequestHash != requestHash)
        {
            return new IdempotencyCheckResult(true, false, null, null, HashMismatch: true);
        }

        // Cache for fast subsequent lookups
        await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromHours(1), cancellationToken: cancellationToken);

        return new IdempotencyCheckResult(true, record.IsProcessing, record.ResponseStatusCode, record.ResponseBody, false);
    }

    public async Task RegisterProcessingAsync(
        string key,
        string path,
        string method,
        string requestHash,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var record = new IdempotentRequest
        {
            Key = key,
            Path = path,
            Method = method,
            RequestHash = requestHash,
            IsProcessing = true,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(24))
        };

        await _context.IdempotentRequests.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var cacheKey = $"{CachePrefix}{key}";
        await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromMinutes(5), cancellationToken: cancellationToken);

        _logger.LogInformation("Registered in-flight idempotency key {IdempotencyKey} for {Method} {Path}", key, method, path);
    }

    public async Task SaveResponseAsync(
        string key,
        int statusCode,
        string responseBody,
        CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotentRequests
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        if (record != null)
        {
            record.IsProcessing = false;
            record.ResponseStatusCode = statusCode;
            record.ResponseBody = responseBody;

            await _context.SaveChangesAsync(cancellationToken);

            var cacheKey = $"{CachePrefix}{key}";
            await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromHours(24), cancellationToken: cancellationToken);

            _logger.LogInformation("Saved idempotent response for key {IdempotencyKey} with status {StatusCode}", key, statusCode);
        }
    }

    public async Task MarkFailedAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotentRequests
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        if (record != null)
        {
            _context.IdempotentRequests.Remove(record);
            await _context.SaveChangesAsync(cancellationToken);

            var cacheKey = $"{CachePrefix}{key}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogWarning("Removed failed idempotent execution record for key {IdempotencyKey}", key);
        }
    }
}
