using ECommerce.Infrastructure.Services.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ECommerce.UnitTests;

public class CacheTests
{
    private readonly CacheService _cacheService;

    public CacheTests()
    {
        var memoryDistributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _cacheService = new CacheService(memoryDistributedCache, NullLogger<CacheService>.Instance);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_ShouldRetrieveStoredValue()
    {
        var key = "test:user:123";
        var data = new TestUserCacheDto { Name = "Jane Tester", Role = "Admin" };

        await _cacheService.SetAsync(key, data, TimeSpan.FromMinutes(10));

        var exists = await _cacheService.ExistsAsync(key);
        exists.Should().BeTrue();

        var retrieved = await _cacheService.GetAsync<TestUserCacheDto>(key);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Jane Tester");
        retrieved.Role.Should().Be("Admin");
    }

    private class TestUserCacheDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldInvokeFactoryOnlyOnMiss()
    {
        var key = "test:computed:val";
        var invocationCount = 0;

        Task<string> Factory()
        {
            invocationCount++;
            return Task.FromResult("ComputedValue");
        }

        // First call: Miss -> Invokes factory
        var res1 = await _cacheService.GetOrSetAsync(key, Factory);
        res1.Should().Be("ComputedValue");
        invocationCount.Should().Be(1);

        // Second call: Hit -> Returns cached without invoking factory
        var res2 = await _cacheService.GetOrSetAsync(key, Factory);
        res2.Should().Be("ComputedValue");
        invocationCount.Should().Be(1);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_ShouldEvictAllMatchingKeys()
    {
        await _cacheService.SetAsync("catalog:product:1", "P1");
        await _cacheService.SetAsync("catalog:product:2", "P2");
        await _cacheService.SetAsync("settings:tax", "TaxRate");

        await _cacheService.RemoveByPrefixAsync("catalog:product:");

        var p1 = await _cacheService.GetAsync<string>("catalog:product:1");
        var p2 = await _cacheService.GetAsync<string>("catalog:product:2");
        var tax = await _cacheService.GetAsync<string>("settings:tax");

        p1.Should().BeNull();
        p2.Should().BeNull();
        tax.Should().Be("TaxRate");
    }
}
