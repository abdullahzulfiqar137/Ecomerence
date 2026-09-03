using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CatalogApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CatalogApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ApiEndpoint_ShouldReturnSuccessfulPagedList()
    {
        var response = await _client.GetAsync("/api/v1/products?page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductListDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategories_ApiEndpoint_ShouldReturnCategoriesList()
    {
        var response = await _client.GetAsync("/api/v1/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CategoryDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBrands_ApiEndpoint_ShouldReturnBrandsList()
    {
        var response = await _client.GetAsync("/api/v1/brands");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<BrandDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
