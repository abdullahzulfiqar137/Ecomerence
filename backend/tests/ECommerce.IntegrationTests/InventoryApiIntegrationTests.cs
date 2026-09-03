using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Inventory;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class InventoryApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InventoryApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllWarehouses_ApiEndpoint_ShouldReturnWarehouses()
    {
        var response = await _client.GetAsync("/api/v1/warehouses");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<WarehouseDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Reserve_And_Release_Inventory_ViaApi_ShouldSucceed()
    {
        // 1. Get warehouses
        var whResponse = await _client.GetAsync("/api/v1/warehouses");
        var whResult = await whResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<WarehouseDto>>>();
        var warehouseId = whResult!.Data![0].Id;

        // 2. Get a simple product (Phone or Headphones)
        var prodResponse = await _client.GetAsync("/api/v1/products?type=1&page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ECommerce.Application.DTOs.Catalog.ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 3. Reserve stock
        var reserveDto = new CreateReservationRequestDto
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            Quantity = 1,
            HoldDurationMinutes = 15,
            CartSessionId = "integration_test_session"
        };

        var resResponse = await _client.PostAsJsonAsync("/api/v1/inventory/reserve", reserveDto);
        var resContent = await resResponse.Content.ReadAsStringAsync();
        resResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: resContent);

        var resResult = await resResponse.Content.ReadFromJsonAsync<ApiResponse<InventoryReservationDto>>();
        resResult.Should().NotBeNull();
        resResult!.Success.Should().BeTrue();

        // 4. Release stock
        var relResponse = await _client.PostAsync($"/api/v1/inventory/release/{resResult.Data!.Id}", null);
        relResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
