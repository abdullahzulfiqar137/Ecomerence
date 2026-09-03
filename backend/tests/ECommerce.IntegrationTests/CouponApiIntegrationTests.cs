using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CouponApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CouponApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ValidateCoupon_ApiEndpoint_ShouldCalculateDiscount()
    {
        var request = new ValidateCouponRequestDto
        {
            Code = "WELCOME10",
            OrderSubtotal = 100.00m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/coupons/validate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ValidateCouponResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.CalculatedDiscountAmount.Should().Be(10.00m);
        result.Data.NewSubtotal.Should().Be(90.00m);
    }

    [Fact]
    public async Task Admin_Create_And_Get_Coupon_ApiEndpoint_ShouldSucceed()
    {
        // 1. Login as Admin
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new ECommerce.Application.DTOs.Auth.LoginRequestDto
        {
            Email = "admin@ecommerce.global",
            Password = "AdminPassword123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ECommerce.Application.DTOs.Auth.AuthResponseDto>>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);

        // 2. Create Coupon
        var uniqueCode = $"PROMO{Guid.NewGuid():N}"[..10].ToUpperInvariant();
        var createDto = new CreateCouponDto
        {
            Code = uniqueCode,
            Description = "Integration Test Promo",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 25m,
            MinimumOrderAmount = 75m,
            StartsAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(1),
            UsageLimitTotal = 100,
            UsageLimitPerCustomer = 2,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/coupons", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CouponDto>>();
        createResult.Should().NotBeNull();
        createResult!.Success.Should().BeTrue();
        createResult.Data!.Code.Should().Be(uniqueCode);

        // 3. Get By ID
        var getResponse = await _client.GetAsync($"/api/v1/coupons/{createResult.Data.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
