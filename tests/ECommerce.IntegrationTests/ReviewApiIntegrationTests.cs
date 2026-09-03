using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class ReviewApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReviewApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProductReviews_And_Summary_ApiEndpoint_ShouldSucceed()
    {
        // 1. Get a product
        var prodResponse = await _client.GetAsync("/api/v1/products?type=1&page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 2. Get reviews for product
        var reviewsResponse = await _client.GetAsync($"/api/v1/reviews/products/{productId}");
        reviewsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var reviewsResult = await reviewsResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductReviewDto>>>();
        reviewsResult.Should().NotBeNull();
        reviewsResult!.Success.Should().BeTrue();

        // 3. Get review summary
        var summaryResponse = await _client.GetAsync($"/api/v1/reviews/products/{productId}/summary");
        summaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var summaryResult = await summaryResponse.Content.ReadFromJsonAsync<ApiResponse<ProductReviewSummaryDto>>();
        summaryResult.Should().NotBeNull();
        summaryResult!.Success.Should().BeTrue();
        summaryResult.Data!.ProductId.Should().Be(productId);
    }
}
