using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Notifications;
using ECommerce.Infrastructure.Services.Orders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class NotificationTests
{
    private readonly ApplicationDbContext _context;
    private readonly EmailTemplateEngine _templateEngine;
    private readonly NotificationService _notificationService;
    private readonly OrderService _orderService;

    public NotificationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _templateEngine = new EmailTemplateEngine();
        var emailSender = new EmailSender(new ConfigurationBuilder().Build(), NullLogger<EmailSender>.Instance);
        _notificationService = new NotificationService(_context, _templateEngine, emailSender, NullLogger<NotificationService>.Instance);
        _orderService = new OrderService(_context, new OrderStateMachine(), NullLogger<OrderService>.Instance);
    }

    [Fact]
    public void EmailTemplateEngine_OrderConfirmation_ShouldContainOrderDetails()
    {
        var order = new Domain.Entities.Order
        {
            OrderNumber = "ORD-TEST-1234",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane.doe@example.com",
            CustomerPhone = "+12025550144",
            TotalAmount = 299.99m,
            SubtotalAmount = 270.00m,
            ShippingAmount = 15.00m,
            TaxAmount = 14.99m,
            Currency = "USD",
            Items = new List<Domain.Entities.OrderItem>
            {
                new()
                {
                    ProductName = "Wireless Noise Cancelling Headphones",
                    SKU = "HDPH-001",
                    Quantity = 1,
                    UnitPrice = 270.00m,
                    TotalLineAmount = 270.00m
                }
            }
        };

        var html = _templateEngine.RenderOrderConfirmation(order);

        html.Should().Contain("ORD-TEST-1234");
        html.Should().Contain("Jane Doe");
        html.Should().Contain("Wireless Noise Cancelling Headphones");
        html.Should().Contain("$299.99");
    }

    [Fact]
    public async Task SendCustomEmail_And_MarkAsRead_ShouldTrackOutbox()
    {
        var user = await _context.Users.FirstAsync();

        var sendRequest = new SendEmailRequestDto
        {
            UserId = user.Id,
            RecipientEmail = user.Email,
            RecipientName = $"{user.FirstName} {user.LastName}",
            Subject = "Important Account Update",
            BodyHtml = "<p>Your security settings have been updated.</p>",
            Type = NotificationType.WelcomeCustomer
        };

        var sendResponse = await _notificationService.SendCustomEmailAsync(sendRequest);

        sendResponse.Success.Should().BeTrue();
        sendResponse.Data.Should().NotBeNull();
        sendResponse.Data!.Status.Should().Be(NotificationStatus.Sent);

        var notificationId = sendResponse.Data.Id;

        // Verify outbox listing
        var outbox = await _notificationService.GetOutboxMessagesAsync(NotificationStatus.Sent);
        outbox.Data!.Items.Should().Contain(n => n.Id == notificationId);

        // Mark as read
        var readRes = await _notificationService.MarkAsReadAsync(notificationId, user.Id);
        readRes.Success.Should().BeTrue();

        var updatedNotification = await _context.Notifications.FindAsync(notificationId);
        updatedNotification!.IsRead.Should().BeTrue();
        updatedNotification.ReadAtUtc.Should().NotBeNull();
    }
}
