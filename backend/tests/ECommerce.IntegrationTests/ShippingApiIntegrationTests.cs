using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.DTOs.Shipping;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class ShippingApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ShippingApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task EstimateShippingRates_ApiEndpoint_ShouldReturnMethods()
    {
        var request = new ShippingEstimateRequestDto
        {
            CountryCode = "US",
            SubtotalAmount = 120m,
            TotalWeightKg = 1.2m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/shipping/estimate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ShippingEstimateResponseDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateShipment_And_TrackByNumber_ApiEndpoint_ShouldSucceed()
    {
        // 1. Get a product
        var prodResponse = await _client.GetAsync("/api/v1/products?type=1&page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ECommerce.Application.DTOs.Catalog.ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 2. Create Order
        var orderResponse = await _client.PostAsJsonAsync("/api/v1/orders", new CreateOrderDto
        {
            CustomerName = "Shipping API Customer",
            CustomerEmail = "shippingapi@test.com",
            CustomerPhone = "+12025550166",
            Currency = "USD",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Ship",
                LastName = "API",
                StreetLine1 = "100 Shipping Way",
                City = "Houston",
                StateOrProvince = "TX",
                PostalCode = "77001",
                CountryCode = "US"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        });

        var orderResult = await orderResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        var orderId = orderResult!.Data!.Id;

        // 3. Login as Admin to get Bearer token
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new ECommerce.Application.DTOs.Auth.LoginRequestDto
        {
            Email = "admin@ecommerce.global",
            Password = "AdminPassword123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ECommerce.Application.DTOs.Auth.AuthResponseDto>>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);

        // 4. Create Shipment
        var shipmentDto = new CreateShipmentDto
        {
            OrderId = orderId,
            CarrierName = "DHL",
            ShippingCost = 14.99m
        };

        var shipmentResponse = await _client.PostAsJsonAsync("/api/v1/shipping/shipments", shipmentDto);
        shipmentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var shipmentResult = await shipmentResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentDto>>();
        shipmentResult.Should().NotBeNull();
        shipmentResult!.Success.Should().BeTrue();
        var trackingNumber = shipmentResult.Data!.TrackingNumber;

        // 4. Track by tracking number
        var trackResponse = await _client.GetAsync($"/api/v1/shipping/track/{trackingNumber}");
        trackResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var trackResult = await trackResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentDto>>();
        trackResult!.Data!.TrackingNumber.Should().Be(trackingNumber);
        trackResult.Data.TrackingEvents.Should().NotBeEmpty();
    }
}
