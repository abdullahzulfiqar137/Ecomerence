using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Jobs;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(
        ApplicationDbContext context,
        IEmailSender emailSender,
        ILogger<BackgroundJobService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ApiResponse<int>> ReleaseExpiredStockHoldsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredHolds = await _context.InventoryReservations
            .Include(r => r.InventoryItem)
            .Where(r => !r.IsReleased && !r.IsCommitted && r.ExpiresAtUtc < now)
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var hold in expiredHolds)
        {
            hold.IsReleased = true;
            if (hold.InventoryItem != null)
            {
                hold.InventoryItem.QuantityReserved = Math.Max(0, hold.InventoryItem.QuantityReserved - hold.Quantity);
            }
            count++;
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Released {Count} expired inventory reservations and restored available warehouse stock.", count);
        }

        return ApiResponse<int>.SuccessResponse(count, $"Successfully released {count} expired stock holds.");
    }

    public async Task<ApiResponse<int>> CleanupAbandonedGuestCartsAsync(int olderThanDays = 30, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
        var staleGuestCarts = await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.UserId == null && ((c.ExpiresAtUtc.HasValue && c.ExpiresAtUtc.Value < DateTime.UtcNow) || c.CreatedAtUtc < cutoff))
            .ToListAsync(cancellationToken);

        var count = staleGuestCarts.Count;
        if (count > 0)
        {
            _context.Carts.RemoveRange(staleGuestCarts);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} abandoned guest carts older than {Days} days.", count, olderThanDays);
        }

        return ApiResponse<int>.SuccessResponse(count, $"Cleaned up {count} abandoned guest carts.");
    }

    public async Task<ApiResponse<int>> DeactivateExpiredCouponsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredCoupons = await _context.Coupons
            .Where(c => c.IsActive && c.ExpiresAtUtc.HasValue && c.ExpiresAtUtc.Value < now)
            .ToListAsync(cancellationToken);

        var count = expiredCoupons.Count;
        if (count > 0)
        {
            foreach (var coupon in expiredCoupons)
            {
                coupon.IsActive = false;
            }
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deactivated {Count} expired coupons.", count);
        }

        return ApiResponse<int>.SuccessResponse(count, $"Deactivated {count} expired coupons.");
    }

    public async Task<ApiResponse<int>> SyncLowStockAlertsAsync(CancellationToken cancellationToken = default)
    {
        var lowStockItems = await _context.InventoryItems
            .Include(i => i.Product)
            .Where(i => (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderPoint)
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var item in lowStockItems)
        {
            var available = item.QuantityOnHand - item.QuantityReserved;
            var exists = await _context.LowStockAlerts
                .AnyAsync(a => a.InventoryItemId == item.Id && !a.IsResolved, cancellationToken);

            if (!exists)
            {
                var alert = new LowStockAlert
                {
                    InventoryItemId = item.Id,
                    CurrentQuantity = available,
                    Threshold = item.ReorderPoint,
                    IsResolved = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _context.LowStockAlerts.AddAsync(alert, cancellationToken);
                count++;
            }
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Generated {Count} new low stock alerts.", count);
        }

        return ApiResponse<int>.SuccessResponse(count, $"Synced low stock items. Generated {count} new alerts.");
    }

    public async Task<ApiResponse<int>> ProcessPendingOutboxNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending || (n.Status == NotificationStatus.Failed && n.RetryCount < 3))
            .OrderBy(n => n.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        var processedCount = 0;
        foreach (var notif in pending)
        {
            notif.RetryCount++;
            var sent = await _emailSender.SendEmailAsync(notif.RecipientEmail, notif.RecipientName, notif.Subject, notif.BodyHtml, cancellationToken);
            if (sent)
            {
                notif.Status = NotificationStatus.Sent;
                notif.SentAtUtc = DateTime.UtcNow;
                notif.ErrorMessage = null;
            }
            else
            {
                notif.Status = NotificationStatus.Failed;
                notif.ErrorMessage = "Worker dispatch failed.";
            }

            processedCount++;
        }

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {Count} pending outbox notifications.", processedCount);
        }

        return ApiResponse<int>.SuccessResponse(processedCount, $"Processed {processedCount} outbox notifications.");
    }
}
