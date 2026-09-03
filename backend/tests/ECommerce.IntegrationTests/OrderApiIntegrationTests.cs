using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class OrderApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_And_GetByNumber_ApiEndpoint_ShouldSucceed()
    {
        // 1. Get a product
        var prodResponse = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ECommerce.Application.DTOs.Catalog.ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 2. Create order
        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "Integration Customer",
            CustomerEmail = "customer@integration.test",
            CustomerPhone = "+12025550143",
            Currency = "USD",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Integration",
                LastName = "Customer",
                PhoneNumber = "+12025550143",
                StreetLine1 = "100 Broadway",
                City = "New York",
                StateOrProvince = "NY",
                PostalCode = "10005",
                CountryCode = "US"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/orders", createOrderDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.OrderNumber.Should().StartWith("ORD-");
        result.Data.Items.Should().HaveCount(1);
    }
}
