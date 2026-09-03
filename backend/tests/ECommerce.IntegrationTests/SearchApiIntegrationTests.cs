using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Search;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class SearchApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SearchApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchProducts_And_Suggestions_ApiEndpoint_ShouldSucceed()
    {
        // 1. Search Products
        var searchResponse = await _client.GetAsync("/api/v1/search?query=Pro&page=1&pageSize=10");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchResult = await searchResponse.Content.ReadFromJsonAsync<ApiResponse<SearchResponseDto>>();
        searchResult.Should().NotBeNull();
        searchResult!.Success.Should().BeTrue();
        searchResult.Data!.Facets.Should().NotBeNull();

        // 2. Suggestions
        var suggestResponse = await _client.GetAsync("/api/v1/search/suggestions?q=Lap");
        suggestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var suggestResult = await suggestResponse.Content.ReadFromJsonAsync<ApiResponse<SearchSuggestionDto>>();
        suggestResult.Should().NotBeNull();
        suggestResult!.Success.Should().BeTrue();
    }
}
