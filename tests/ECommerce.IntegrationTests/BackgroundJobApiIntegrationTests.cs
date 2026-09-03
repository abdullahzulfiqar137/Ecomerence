using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class BackgroundJobApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BackgroundJobApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Release_Holds_And_Job_Status_ApiEndpoint_ShouldSucceed()
    {
        // 1. Login as Admin
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new ECommerce.Application.DTOs.Auth.LoginRequestDto
        {
            Email = "admin@ecommerce.global",
            Password = "AdminPassword123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ECommerce.Application.DTOs.Auth.AuthResponseDto>>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);

        // 2. Trigger Release Holds
        var releaseResponse = await _client.PostAsync("/api/v1/admin/jobs/release-holds", null);
        releaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var releaseResult = await releaseResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        releaseResult.Should().NotBeNull();
        releaseResult!.Success.Should().BeTrue();

        // 3. Query Jobs Status
        var statusResponse = await _client.GetAsync("/api/v1/admin/jobs/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
