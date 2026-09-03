using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CacheApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CacheApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Cache_Status_And_Clear_Prefix_ApiEndpoint_ShouldSucceed()
    {
        // 1. Login as Admin
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new ECommerce.Application.DTOs.Auth.LoginRequestDto
        {
            Email = "admin@ecommerce.global",
            Password = "AdminPassword123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ECommerce.Application.DTOs.Auth.AuthResponseDto>>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);

        // 2. Cache Status
        var statusResponse = await _client.GetAsync("/api/v1/admin/cache/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Clear Catalog Prefix
        var clearResponse = await _client.PostAsync("/api/v1/admin/cache/clear-catalog", null);
        clearResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clearResult = await clearResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        clearResult.Should().NotBeNull();
        clearResult!.Success.Should().BeTrue();
    }
}
