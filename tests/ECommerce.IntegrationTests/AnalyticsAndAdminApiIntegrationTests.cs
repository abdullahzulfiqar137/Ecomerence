using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Analytics;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class AnalyticsAndAdminApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AnalyticsAndAdminApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Dashboard_Kpi_And_User_List_ApiEndpoint_ShouldSucceed()
    {
        // 1. Login as Admin
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new ECommerce.Application.DTOs.Auth.LoginRequestDto
        {
            Email = "admin@ecommerce.global",
            Password = "AdminPassword123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ECommerce.Application.DTOs.Auth.AuthResponseDto>>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);

        // 2. Get Dashboard KPI
        var kpiResponse = await _client.GetAsync("/api/v1/admin/analytics/dashboard");
        kpiResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var kpiResult = await kpiResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardKpiDto>>();
        kpiResult.Should().NotBeNull();
        kpiResult!.Success.Should().BeTrue();
        kpiResult.Data!.TotalCustomers.Should().BeGreaterThan(0);

        // 3. Get User List
        var usersResponse = await _client.GetAsync("/api/v1/admin/users?page=1&pageSize=10");
        usersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usersResult = await usersResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AdminUserListDto>>>();
        usersResult.Should().NotBeNull();
        usersResult!.Success.Should().BeTrue();
        usersResult.Data!.Items.Should().NotBeEmpty();
    }
}
