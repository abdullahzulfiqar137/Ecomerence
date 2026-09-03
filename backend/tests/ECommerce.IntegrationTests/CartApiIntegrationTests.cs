using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CartApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CartApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GuestCart_Add_Update_SaveForLater_And_Clear_ShouldSucceed()
    {
        var guestSession = Guid.NewGuid().ToString("N");

        // 1. Get a product
        var prodResponse = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ECommerce.Application.DTOs.Catalog.ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 2. Add to guest cart
        var addDto = new AddToCartRequestDto
        {
            ProductId = productId,
            Quantity = 2,
            GuestSessionId = guestSession
        };

        var addResponse = await _client.PostAsJsonAsync("/api/v1/cart/items", addDto);
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var addResult = await addResponse.Content.ReadFromJsonAsync<ApiResponse<CartDto>>();
        addResult.Should().NotBeNull();
        addResult!.Success.Should().BeTrue();
        addResult.Data!.Items.Should().HaveCount(1);
        var cartItemId = addResult.Data.Items[0].Id;

        // 3. Toggle Save For Later
        var saveResponse = await _client.PostAsync($"/api/v1/cart/items/{cartItemId}/save-for-later?guestSessionId={guestSession}", null);
        saveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var saveResult = await saveResponse.Content.ReadFromJsonAsync<ApiResponse<CartDto>>();
        saveResult!.Data!.SavedForLaterItems.Should().HaveCount(1);
        saveResult.Data.Items.Should().BeEmpty();

        // 4. Toggle back to cart
        var moveBackResponse = await _client.PostAsync($"/api/v1/cart/items/{cartItemId}/save-for-later?guestSessionId={guestSession}", null);
        moveBackResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Clear cart
        var clearResponse = await _client.DeleteAsync($"/api/v1/cart/clear?guestSessionId={guestSession}");
        clearResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
