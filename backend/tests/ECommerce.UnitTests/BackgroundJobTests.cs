using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Jobs;
using ECommerce.Infrastructure.Services.Notifications;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class BackgroundJobTests
{
    private readonly ApplicationDbContext _context;
    private readonly BackgroundJobService _jobService;

    public BackgroundJobTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        var emailSender = new EmailSender(new ConfigurationBuilder().Build(), NullLogger<EmailSender>.Instance);
        _jobService = new BackgroundJobService(_context, emailSender, NullLogger<BackgroundJobService>.Instance);
    }

    [Fact]
    public async Task ReleaseExpiredStockHoldsAsync_ShouldFreeExpiredReservations()
    {
        var inventoryItem = await _context.InventoryItems.FirstAsync();
        inventoryItem.QuantityOnHand = 20;
        inventoryItem.QuantityReserved = 5;

        var expiredReservation = new InventoryReservation
        {
            InventoryItemId = inventoryItem.Id,
            Quantity = 3,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-10), // Expired
            IsReleased = false,
            IsCommitted = false
        };

        await _context.InventoryReservations.AddAsync(expiredReservation);
        await _context.SaveChangesAsync();

        var response = await _jobService.ReleaseExpiredStockHoldsAsync();

        response.Success.Should().BeTrue();
        response.Data.Should().BeGreaterThan(0);

        var updatedRes = await _context.InventoryReservations.FindAsync(expiredReservation.Id);
        updatedRes!.IsReleased.Should().BeTrue();

        var updatedItem = await _context.InventoryItems.FindAsync(inventoryItem.Id);
        updatedItem!.QuantityReserved.Should().Be(2); // 5 - 3 = 2
    }

    [Fact]
    public async Task DeactivateExpiredCouponsAsync_ShouldDisableStaleCoupons()
    {
        var expiredCoupon = new Coupon
        {
            Code = "EXPIRED99",
            Description = "Stale Coupon",
            DiscountValue = 10,
            IsActive = true,
            StartsAtUtc = DateTime.UtcNow.AddMonths(-2),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Coupons.AddAsync(expiredCoupon);
        await _context.SaveChangesAsync();

        var response = await _jobService.DeactivateExpiredCouponsAsync();

        response.Success.Should().BeTrue();
        response.Data.Should().BeGreaterThan(0);

        var updated = await _context.Coupons.FindAsync(expiredCoupon.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SyncLowStockAlertsAsync_ShouldCreateAlertsForDepletedItems()
    {
        var item = await _context.InventoryItems.FirstAsync();
        item.QuantityOnHand = 2;
        item.QuantityReserved = 0;
        item.ReorderPoint = 10;
        await _context.SaveChangesAsync();

        var response = await _jobService.SyncLowStockAlertsAsync();

        response.Success.Should().BeTrue();
        response.Data.Should().BeGreaterThan(0);

        var alert = await _context.LowStockAlerts.FirstOrDefaultAsync(a => a.InventoryItemId == item.Id && !a.IsResolved);
        alert.Should().NotBeNull();
        alert!.CurrentQuantity.Should().Be(2);
    }
}
