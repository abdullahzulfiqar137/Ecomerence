using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.DTOs.Shipping;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Orders;
using ECommerce.Infrastructure.Services.Shipping;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class ShippingTests
{
    private readonly ApplicationDbContext _context;
    private readonly ShippingService _shippingService;
    private readonly OrderService _orderService;

    public ShippingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _shippingService = new ShippingService(_context, NullLogger<ShippingService>.Instance);
        _orderService = new OrderService(_context, new OrderStateMachine(), NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task EstimateShippingRatesAsync_WeightTierAndFreeThreshold_ShouldCalculateAccurately()
    {
        // 1. Estimate with 2 kg weight, $20 subtotal -> Standard: 5.00 + (2 * 1.50) = 8.00
        var estimate1 = await _shippingService.EstimateShippingRatesAsync(new ShippingEstimateRequestDto
        {
            CountryCode = "PK",
            SubtotalAmount = 20m,
            TotalWeightKg = 2m
        });

        estimate1.Success.Should().BeTrue();
        estimate1.Data.Should().NotBeNull();
        estimate1.Data!.Should().NotBeEmpty();

        var stdRate = estimate1.Data!.First(r => r.MethodName.Contains("Standard"));
        stdRate.ShippingCost.Should().Be(8.00m);
        stdRate.IsFreeShipping.Should().BeFalse();

        // 2. Estimate with subtotal $100 -> Free shipping threshold ($50) reached for Standard
        var estimateFree = await _shippingService.EstimateShippingRatesAsync(new ShippingEstimateRequestDto
        {
            CountryCode = "PK",
            SubtotalAmount = 100m,
            TotalWeightKg = 2m
        });

        var stdFreeRate = estimateFree.Data!.First(r => r.MethodName.Contains("Standard"));
        stdFreeRate.ShippingCost.Should().Be(0m);
        stdFreeRate.IsFreeShipping.Should().BeTrue();
    }

    [Fact]
    public async Task CreateShipment_And_AddTrackingEvents_ShouldCascadeOrderStatus()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        var orderRes = await _orderService.CreateOrderAsync(user.Id, new CreateOrderDto
        {
            CustomerName = "Shipment Customer",
            CustomerEmail = "shipment@test.com",
            CustomerPhone = "+923009988776",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Ship",
                LastName = "Customer",
                StreetLine1 = "Street 5",
                City = "Lahore",
                CountryCode = "PK"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        });

        var orderId = orderRes.Data!.Id;

        // 1. Create Shipment (Dispatched)
        var shipmentRes = await _shippingService.CreateShipmentAsync(new CreateShipmentDto
        {
            OrderId = orderId,
            CarrierName = "TCS",
            ShippingCost = 250m
        });

        shipmentRes.Success.Should().BeTrue();
        shipmentRes.Data!.TrackingNumber.Should().StartWith("TCS");
        shipmentRes.Data.Status.Should().Be(ShipmentStatus.InTransit);

        // Check Order status cascaded
        var order = await _context.Orders.FindAsync(orderId);
        order!.ShippingStatus.Should().Be(ShipmentStatus.InTransit);

        // 2. Add Tracking Event: Delivered
        var trackRes = await _shippingService.AddTrackingEventAsync(shipmentRes.Data.Id, new AddTrackingEventDto
        {
            Status = ShipmentStatus.Delivered,
            Location = "Lahore Destination Hub",
            Description = "Delivered to recipient."
        });

        trackRes.Success.Should().BeTrue();
        trackRes.Data!.Status.Should().Be(ShipmentStatus.Delivered);
        trackRes.Data.DeliveredAtUtc.Should().NotBeNull();

        // Verify Order is now Delivered
        order = await _context.Orders.FindAsync(orderId);
        order!.ShippingStatus.Should().Be(ShipmentStatus.Delivered);
        order.Status.Should().Be(OrderStatus.Delivered);
    }
}
