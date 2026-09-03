using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Orders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class OrderTests
{
    private readonly ApplicationDbContext _context;
    private readonly OrderStateMachine _stateMachine;
    private readonly OrderService _orderService;

    public OrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _stateMachine = new OrderStateMachine();
        _orderService = new OrderService(_context, _stateMachine, NullLogger<OrderService>.Instance);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Processing, OrderStatus.Packed, true)]
    [InlineData(OrderStatus.Packed, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Refunded, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Processing, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing, false)]
    public void OrderStateMachine_ValidateTransitions(OrderStatus from, OrderStatus to, bool expectedResult)
    {
        var canTransition = _stateMachine.CanTransition(from, to);
        canTransition.Should().Be(expectedResult);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ShouldCreateOrderWithImmutableSnapshots()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john.doe@example.com",
            CustomerPhone = "+923001234567",
            Currency = "USD",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+923001234567",
                StreetLine1 = "Plot 45, Main Boulevard",
                City = "Lahore",
                StateOrProvince = "Punjab",
                PostalCode = "54000",
                CountryCode = "PK"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        };

        var response = await _orderService.CreateOrderAsync(user.Id, createOrderDto);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.OrderNumber.Should().StartWith("ORD-");
        response.Data.Status.Should().Be(OrderStatus.Pending);
        response.Data.Items.Should().HaveCount(1);
        response.Data.Items[0].ProductName.Should().Be(product.Name);
        response.Data.Items[0].SKU.Should().Be(product.SKU);
        response.Data.StatusHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidTransition_ShouldUpdateAndRecordHistory()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "Jane Doe",
            CustomerEmail = "jane.doe@example.com",
            CustomerPhone = "+923007654321",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                PhoneNumber = "+923007654321",
                StreetLine1 = "Street 10",
                City = "Karachi",
                StateOrProvince = "Sindh",
                PostalCode = "75500",
                CountryCode = "PK"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        };

        var orderResponse = await _orderService.CreateOrderAsync(user.Id, createOrderDto);
        var orderId = orderResponse.Data!.Id;

        // Transition: Pending -> Confirmed
        var updateResult = await _orderService.UpdateOrderStatusAsync(orderId, new UpdateOrderStatusDto
        {
            NewStatus = OrderStatus.Confirmed,
            Comment = "Payment verified and order confirmed."
        }, "Admin");

        updateResult.Success.Should().BeTrue();
        updateResult.Data!.Status.Should().Be(OrderStatus.Confirmed);
        updateResult.Data.StatusHistory.Should().HaveCount(2);

        // Invalid transition: Confirmed -> Delivered
        var invalidResult = await _orderService.UpdateOrderStatusAsync(orderId, new UpdateOrderStatusDto
        {
            NewStatus = OrderStatus.Delivered
        }, "Admin");

        invalidResult.Success.Should().BeFalse();
        invalidResult.Message.Should().Be("Invalid Transition");
    }
}
