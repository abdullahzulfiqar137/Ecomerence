using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        IEmailTemplateEngine templateEngine,
        IEmailSender emailSender,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _templateEngine = templateEngine;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ApiResponse<NotificationDto>> SendCustomEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            RecipientEmail = request.RecipientEmail.Trim(),
            RecipientName = request.RecipientName.Trim(),
            Type = request.Type,
            Subject = request.Subject.Trim(),
            BodyHtml = request.BodyHtml,
            Status = NotificationStatus.Pending,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            CreatedAtUtc = DateTime.UtcNow
        };

        var sentSuccess = await _emailSender.SendEmailAsync(notification.RecipientEmail, notification.RecipientName, notification.Subject, notification.BodyHtml, cancellationToken);
        if (sentSuccess)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAtUtc = DateTime.UtcNow;
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = "Failed to dispatch email via SMTP provider.";
        }

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<NotificationDto>.SuccessResponse(MapToDto(notification));
    }

    public async Task<ApiResponse<NotificationDto>> SendOrderConfirmationAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            return ApiResponse<NotificationDto>.FailureResponse("Not Found", "Order not found.");
        }

        var html = _templateEngine.RenderOrderConfirmation(order);
        var subject = $"Order Confirmation - {order.OrderNumber}";

        var request = new SendEmailRequestDto
        {
            RecipientEmail = order.CustomerEmail,
            RecipientName = order.CustomerName,
            Subject = subject,
            BodyHtml = html,
            Type = NotificationType.OrderConfirmed,
            UserId = order.UserId,
            ReferenceId = order.Id,
            ReferenceType = "Order"
        };

        return await SendCustomEmailAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<NotificationDto>> SendShipmentDispatchedAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipmentId, cancellationToken);

        if (shipment == null || shipment.Order == null)
        {
            return ApiResponse<NotificationDto>.FailureResponse("Not Found", "Shipment or associated order not found.");
        }

        var html = _templateEngine.RenderShipmentDispatched(shipment);
        var subject = $"Your Order {shipment.Order.OrderNumber} Has Been Shipped! ({shipment.CarrierName})";

        var request = new SendEmailRequestDto
        {
            RecipientEmail = shipment.Order.CustomerEmail,
            RecipientName = shipment.Order.CustomerName,
            Subject = subject,
            BodyHtml = html,
            Type = NotificationType.OrderShipped,
            UserId = shipment.Order.UserId,
            ReferenceId = shipment.Id,
            ReferenceType = "Shipment"
        };

        return await SendCustomEmailAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .AsQueryable();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(n => MapToDto(n)).ToList();
        var pagedResult = new PagedResult<NotificationDto>(dtos, total, page, pageSize);

        return ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Notification not found.");
        }

        notification.IsRead = true;
        notification.ReadAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Notification marked as read.");
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetOutboxMessagesAsync(NotificationStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(n => MapToDto(n)).ToList();
        var pagedResult = new PagedResult<NotificationDto>(dtos, total, page, pageSize);

        return ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<bool>> ResendFailedNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notification == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Notification not found.");
        }

        notification.RetryCount++;
        var sent = await _emailSender.SendEmailAsync(notification.RecipientEmail, notification.RecipientName, notification.Subject, notification.BodyHtml, cancellationToken);
        if (sent)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAtUtc = DateTime.UtcNow;
            notification.ErrorMessage = null;
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = "Retry failed.";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(sent, sent ? "Notification resent successfully." : "Resend attempt failed.");
    }

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            RecipientEmail = n.RecipientEmail,
            RecipientName = n.RecipientName,
            Type = n.Type,
            Subject = n.Subject,
            BodyHtml = n.BodyHtml,
            Status = n.Status,
            SentAtUtc = n.SentAtUtc,
            IsRead = n.IsRead,
            ReadAtUtc = n.ReadAtUtc,
            ReferenceId = n.ReferenceId,
            ReferenceType = n.ReferenceType,
            CreatedAtUtc = n.CreatedAtUtc
        };
    }
}
