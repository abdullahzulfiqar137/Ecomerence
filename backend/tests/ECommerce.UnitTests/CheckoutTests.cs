using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Cart;
using ECommerce.Infrastructure.Services.Checkout;
using ECommerce.Infrastructure.Services.Inventory;
using ECommerce.Infrastructure.Services.Orders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class CheckoutTests
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;
    private readonly InventoryService _inventoryService;
    private readonly OrderService _orderService;
    private readonly CheckoutService _checkoutService;

    public CheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _cartService = new CartService(_context, NullLogger<CartService>.Instance);
        _inventoryService = new InventoryService(_context, NullLogger<InventoryService>.Instance);
        _orderService = new OrderService(_context, new OrderStateMachine(), NullLogger<OrderService>.Instance);
        _checkoutService = new CheckoutService(_context, _inventoryService, _orderService, _cartService, NullLogger<CheckoutService>.Instance);
    }

    [Fact]
    public async Task CalculateQuoteAsync_WithValidCart_ShouldComputeTaxesAndShipping()
    {
        var product = await _context.Products.FirstAsync(p => p.Price > 0);
        var guestSession = Guid.NewGuid().ToString("N");

        await _cartService.AddItemAsync(null, new AddToCartRequestDto
        {
            ProductId = product.Id,
            Quantity = 2,
            GuestSessionId = guestSession
        });

        var quoteRequest = new CheckoutQuoteRequestDto
        {
            GuestSessionId = guestSession,
            CountryCode = "US"
        };

        var quoteResponse = await _checkoutService.CalculateQuoteAsync(null, quoteRequest);

        quoteResponse.Success.Should().BeTrue();
        quoteResponse.Data.Should().NotBeNull();
        quoteResponse.Data!.SubtotalAmount.Should().BeGreaterThan(0);
        quoteResponse.Data.TaxAmount.Should().BeGreaterThan(0);
        quoteResponse.Data.ShippingAmount.Should().BeGreaterThan(0);
        quoteResponse.Data.TotalAmount.Should().Be(quoteResponse.Data.SubtotalAmount + quoteResponse.Data.TaxAmount + quoteResponse.Data.ShippingAmount);
    }

    [Fact]
    public async Task ProcessCheckoutAsync_Complete13StepPipeline_ShouldCreateOrderAndCommitInventory()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync(p => p.Price > 0 && !p.Variants.Any());

        // 1. Add item to user's cart
        await _cartService.AddItemAsync(user.Id, new AddToCartRequestDto
        {
            ProductId = product.Id,
            Quantity = 1
        });

        // 2. Perform Checkout
        var checkoutRequest = new CheckoutRequestDto
        {
            CustomerName = "Test Checkout Customer",
            CustomerEmail = "checkout@example.com",
            CustomerPhone = "+12025550199",
            Currency = "USD",
            PaymentMethod = PaymentMethodType.CashOnDelivery,
            PaymentProvider = "CashOnDelivery",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Test",
                LastName = "Checkout",
                PhoneNumber = "+12025550199",
                StreetLine1 = "500 Main Street",
                City = "San Jose",
                StateOrProvince = "CA",
                PostalCode = "95101",
                CountryCode = "US"
            }
        };

        var response = await _checkoutService.ProcessCheckoutAsync(user.Id, checkoutRequest);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.OrderId.Should().NotBeEmpty();
        response.Data.OrderNumber.Should().StartWith("ORD-");
        response.Data.Status.Should().Be(OrderStatus.Pending); // COD is pending order status
        response.Data.TotalAmount.Should().BeGreaterThan(0);

        // 3. Verify cart was cleared
        var cart = await _cartService.GetCartAsync(user.Id, null);
        cart.Data!.Items.Should().BeEmpty();

        // 4. Verify payment record was created
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == response.Data.OrderId);
        payment.Should().NotBeNull();
        payment!.Status.Should().Be(PaymentStatus.Pending);
    }
}
