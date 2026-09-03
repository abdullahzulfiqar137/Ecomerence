using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class VariantApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VariantApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAttributes_ApiEndpoint_ShouldReturnList()
    {
        var response = await _client.GetAsync("/api/v1/attributes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ProductAttributeDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetVariants_ForProduct_ApiEndpoint_ShouldReturnVariantList()
    {
        // 1. Get products to retrieve a product id
        var prodResponse = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
        prodResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductListDto>>>();
        prodResult.Should().NotBeNull();
        prodResult!.Data!.Items.Should().NotBeEmpty();

        var productId = prodResult.Data.Items[0].Id;

        // 2. Fetch variants for this product
        var response = await _client.GetAsync($"/api/v1/products/{productId}/variants");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VariantDetailDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }
}
