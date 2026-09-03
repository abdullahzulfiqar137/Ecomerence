using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Orders;
using ECommerce.Infrastructure.Services.Payments;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class PaymentTests
{
    private readonly ApplicationDbContext _context;
    private readonly PaymentGatewayFactory _gatewayFactory;
    private readonly PaymentService _paymentService;
    private readonly OrderService _orderService;

    public PaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        var configuration = new ConfigurationBuilder().Build();

        var gateways = new List<IPaymentGateway>
        {
            new StripePaymentGateway(configuration, NullLogger<StripePaymentGateway>.Instance),
            new CashOnDeliveryGateway(),
            new JazzCashPaymentGateway(configuration, NullLogger<JazzCashPaymentGateway>.Instance),
            new EasyPaisaPaymentGateway(configuration, NullLogger<EasyPaisaPaymentGateway>.Instance),
            new PayPalPaymentGateway()
        };

        _gatewayFactory = new PaymentGatewayFactory(gateways);
        _orderService = new OrderService(_context, new OrderStateMachine(), NullLogger<OrderService>.Instance);
        _paymentService = new PaymentService(_context, _gatewayFactory, _orderService, configuration, NullLogger<PaymentService>.Instance);
    }

    [Theory]
    [InlineData("Stripe", "Stripe")]
    [InlineData("JazzCash", "JazzCash")]
    [InlineData("EasyPaisa", "EasyPaisa")]
    [InlineData("PayPal", "PayPal")]
    [InlineData("CashOnDelivery", "CashOnDelivery")]
    public void PaymentGatewayFactory_ShouldResolveCorrectProvider(string providerName, string expectedProvider)
    {
        var gateway = _gatewayFactory.GetGateway(providerName);
        gateway.Should().NotBeNull();
        gateway.ProviderName.Should().Be(expectedProvider);
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_StripeAndJazzCash_ShouldGeneratePayloads()
    {
        var product = await _context.Products.FirstAsync();
        var user = await _context.Users.FirstAsync();

        var orderRes = await _orderService.CreateOrderAsync(user.Id, new CreateOrderDto
        {
            CustomerName = "Payment Test Customer",
            CustomerEmail = "payment@test.com",
            CustomerPhone = "+923001112233",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Payment",
                LastName = "Tester",
                StreetLine1 = "Street 1",
                City = "Islamabad",
                CountryCode = "PK"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        });

        var orderId = orderRes.Data!.Id;

        // 1. Stripe Intent
        var stripeRes = await _paymentService.CreatePaymentIntentAsync(new CreatePaymentIntentRequestDto
        {
            OrderId = orderId,
            Provider = "Stripe"
        });

        stripeRes.Success.Should().BeTrue();
        stripeRes.Data!.ClientSecret.Should().NotBeNullOrEmpty();

        // 2. JazzCash Intent
        var jcRes = await _paymentService.CreatePaymentIntentAsync(new CreatePaymentIntentRequestDto
        {
            OrderId = orderId,
            Provider = "JazzCash"
        });

        jcRes.Success.Should().BeTrue();
        jcRes.Data!.ProviderPayload.Should().ContainKey("pp_SecureHash");
    }

    [Fact]
    public async Task ProcessRefundAsync_ValidCapturedPayment_ShouldUpdateStatuses()
    {
        var order = new Order
        {
            OrderNumber = "ORD-REFUND-001",
            CustomerEmail = "refund@test.com",
            CustomerName = "Refund Customer",
            CustomerPhone = "+12025550111",
            Status = OrderStatus.Delivered,
            PaymentStatus = PaymentStatus.Captured,
            TotalAmount = 150m,
            Currency = "USD"
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            OrderId = order.Id,
            Provider = "Stripe",
            Amount = 150m,
            Currency = "USD",
            Status = PaymentStatus.Captured,
            ExternalTransactionId = "ch_test123"
        };
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        var refundRequest = new RefundRequestDto
        {
            PaymentId = payment.Id,
            Amount = 150m,
            Reason = "Customer Satisfaction Return"
        };

        var response = await _paymentService.ProcessRefundAsync(refundRequest, "FinanceAdmin");

        response.Success.Should().BeTrue();
        response.Data!.RefundTransactionId.Should().NotBeNullOrEmpty();

        // Verify DB updates
        var updatedPayment = await _context.Payments.FindAsync(payment.Id);
        updatedPayment!.Status.Should().Be(PaymentStatus.Refunded);

        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        updatedOrder!.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        updatedOrder.Status.Should().Be(OrderStatus.Refunded);
    }
}
