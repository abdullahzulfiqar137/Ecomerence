using System.Collections.Concurrent;
using System.Text.Json;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private static readonly ConcurrentDictionary<string, byte> KeyRegistry = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve cache key {CacheKey}. Falling back to source.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        if (value == null) return;

        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(30),
                SlidingExpiration = slidingExpiration
            };

            var serialized = JsonSerializer.Serialize(value, JsonOptions);
            await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);

            KeyRegistry.TryAdd(key, 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache key {CacheKey}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await factory();
        if (result != null)
        {
            await SetAsync(key, result, absoluteExpiration, null, cancellationToken);
        }

        return result;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            KeyRegistry.TryRemove(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache key {CacheKey}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var matchingKeys = KeyRegistry.Keys
                .Where(k => k.StartsWith(prefixKey, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in matchingKeys)
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
                KeyRegistry.TryRemove(key, out _);
            }

            _logger.LogInformation("Evicted {Count} cache keys matching prefix '{Prefix}'", matchingKeys.Count, prefixKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache prefix {PrefixKey}", prefixKey);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrEmpty(data);
        }
        catch
        {
            return false;
        }
    }
}
