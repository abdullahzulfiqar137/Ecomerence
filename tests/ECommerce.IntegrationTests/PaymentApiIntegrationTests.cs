using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Models;
using FluentAssertions;
using Xunit;

namespace ECommerce.IntegrationTests;

public class PaymentApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePaymentIntent_And_Webhook_ApiEndpoint_ShouldSucceed()
    {
        // 1. Get a product
        var prodResponse = await _client.GetAsync("/api/v1/products?type=1&page=1&pageSize=1");
        var prodResult = await prodResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ECommerce.Application.DTOs.Catalog.ProductListDto>>>();
        var productId = prodResult!.Data!.Items[0].Id;

        // 2. Create Order
        var orderResponse = await _client.PostAsJsonAsync("/api/v1/orders", new CreateOrderDto
        {
            CustomerName = "Payment API Customer",
            CustomerEmail = "paymentapi@test.com",
            CustomerPhone = "+12025550177",
            Currency = "USD",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Payment",
                LastName = "API",
                StreetLine1 = "500 Payment St",
                City = "Seattle",
                StateOrProvince = "WA",
                PostalCode = "98101",
                CountryCode = "US"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        });

        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var orderResult = await orderResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        var orderId = orderResult!.Data!.Id;

        // 3. Create Stripe Payment Intent
        var intentResponse = await _client.PostAsJsonAsync("/api/v1/payments/intent", new CreatePaymentIntentRequestDto
        {
            OrderId = orderId,
            Provider = "Stripe"
        });

        intentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var intentResult = await intentResponse.Content.ReadFromJsonAsync<ApiResponse<PaymentIntentResponseDto>>();
        intentResult.Should().NotBeNull();
        intentResult!.Success.Should().BeTrue();
        intentResult.Data!.ClientSecret.Should().NotBeNullOrEmpty();

        // 4. Webhook Notification Test
        var webhookContent = new StringContent("{\"event\":\"payment_intent.succeeded\"}", System.Text.Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("Stripe-Signature", "t=123,v1=sample_sig");
        var webhookResponse = await _client.PostAsync("/api/v1/payments/webhook/Stripe", webhookContent);
        webhookResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
