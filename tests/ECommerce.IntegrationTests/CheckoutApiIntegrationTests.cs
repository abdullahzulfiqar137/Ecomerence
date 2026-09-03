using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class CheckoutApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CheckoutApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Guest_Quote_And_Checkout_Pipeline_ApiEndpoint_ShouldSucceed()
    {
        var guestSession = Guid.NewGuid().ToString("N");

        // 1. Get a simple product
        var prodResponse = await _client.GetAsync("/api/v1/products?type=1&page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ECommerce.Application.DTOs.Catalog.ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 2. Add to guest cart
        var addDto = new AddToCartRequestDto
        {
            ProductId = productId,
            Quantity = 1,
            GuestSessionId = guestSession
        };
        var addResponse = await _client.PostAsJsonAsync("/api/v1/cart/items", addDto);
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Get Checkout Quote
        var quoteDto = new CheckoutQuoteRequestDto
        {
            GuestSessionId = guestSession,
            CountryCode = "US"
        };
        var quoteResponse = await _client.PostAsJsonAsync("/api/v1/checkout/quote", quoteDto);
        quoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var quoteResult = await quoteResponse.Content.ReadFromJsonAsync<ApiResponse<CheckoutQuoteResponseDto>>();
        quoteResult.Should().NotBeNull();
        quoteResult!.Success.Should().BeTrue();
        quoteResult.Data!.TotalAmount.Should().BeGreaterThan(0);

        // 4. Process Checkout
        var checkoutDto = new CheckoutRequestDto
        {
            GuestSessionId = guestSession,
            CustomerName = "Guest Shopper",
            CustomerEmail = "guest.shopper@integration.test",
            CustomerPhone = "+12025550188",
            Currency = "USD",
            PaymentMethod = PaymentMethodType.CashOnDelivery,
            PaymentProvider = "CashOnDelivery",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Guest",
                LastName = "Shopper",
                PhoneNumber = "+12025550188",
                StreetLine1 = "742 Evergreen Terrace",
                City = "Springfield",
                StateOrProvince = "OR",
                PostalCode = "97477",
                CountryCode = "US"
            }
        };

        var checkoutResponse = await _client.PostAsJsonAsync("/api/v1/checkout/process", checkoutDto);
        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var checkoutResult = await checkoutResponse.Content.ReadFromJsonAsync<ApiResponse<CheckoutResultDto>>();
        checkoutResult.Should().NotBeNull();
        checkoutResult!.Success.Should().BeTrue();
        checkoutResult.Data!.OrderNumber.Should().StartWith("ORD-");
    }
}
