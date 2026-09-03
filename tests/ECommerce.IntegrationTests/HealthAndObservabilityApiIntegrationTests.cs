using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class HealthAndObservabilityApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthAndObservabilityApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_Endpoints_ShouldReturnHealthyStatus()
    {
        // 1. /health
        var healthRes = await _client.GetAsync("/health");
        healthRes.StatusCode.Should().Be(HttpStatusCode.OK);
        healthRes.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        // 2. /health/live
        var liveRes = await _client.GetAsync("/health/live");
        liveRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. /health/ready
        var readyRes = await _client.GetAsync("/health/ready");
        readyRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SecurityHeaders_And_CorrelationId_ShouldBePresentInResponses()
    {
        var customCorrelationId = "custom-test-correlation-12345";
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/diagnostics/ping");
        request.Headers.Add("X-Correlation-Id", customCorrelationId);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify Correlation ID Echoed
        response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        response.Headers.GetValues("X-Correlation-Id").First().Should().Be(customCorrelationId);

        // Verify Security Headers
        response.Headers.Contains("X-Content-Type-Options").Should().BeTrue();
        response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");

        response.Headers.Contains("X-Frame-Options").Should().BeTrue();
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");
    }

    [Fact]
    public async Task Diagnostics_Info_And_Ping_ApiEndpoints_ShouldSucceed()
    {
        var pingRes = await _client.GetAsync("/api/v1/diagnostics/ping");
        pingRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var pingResult = await pingRes.Content.ReadFromJsonAsync<ApiResponse<string>>();
        pingResult.Should().NotBeNull();
        pingResult!.Data.Should().Be("pong");

        var infoRes = await _client.GetAsync("/api/v1/diagnostics/info");
        infoRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
