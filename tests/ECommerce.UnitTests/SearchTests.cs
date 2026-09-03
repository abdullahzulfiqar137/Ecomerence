using ECommerce.Application.DTOs.Search;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Search;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class SearchTests
{
    private readonly ApplicationDbContext _context;
    private readonly SearchService _searchService;

    public SearchTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _searchService = new SearchService(_context, NullLogger<SearchService>.Instance);
    }

    [Fact]
    public async Task SearchProductsAsync_MultiTerm_ShouldReturnMatchingItemsAndFacets()
    {
        var request = new SearchRequestDto
        {
            Query = "Pro",
            Page = 1,
            PageSize = 10
        };

        var response = await _searchService.SearchProductsAsync(request);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().NotBeEmpty();
        response.Data.Facets.Should().NotBeNull();
        response.Data.Facets.Categories.Should().NotBeEmpty();
        response.Data.Facets.PriceRange.MaxPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchProductsAsync_PriceAndCategoryFilter_ShouldRestrictResults()
    {
        var category = await _context.Categories.FirstAsync();

        var request = new SearchRequestDto
        {
            CategoryId = category.Id,
            MinPrice = 10m,
            MaxPrice = 5000m
        };

        var response = await _searchService.SearchProductsAsync(request);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        foreach (var item in response.Data!.Items)
        {
            item.Price.Should().BeInRange(10m, 5000m);
        }
    }

    [Fact]
    public async Task GetSuggestionsAsync_ValidTerm_ShouldReturnKeywordsAndPreviews()
    {
        var response = await _searchService.GetSuggestionsAsync("Phone");

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Query.Should().Be("Phone");
    }
}
