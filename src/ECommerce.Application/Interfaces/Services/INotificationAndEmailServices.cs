using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

public interface IEmailTemplateEngine
{
    string RenderOrderConfirmation(Order order);
    string RenderShipmentDispatched(Shipment shipment);
    string RenderPaymentReceipt(Payment payment);
    string RenderWelcomeEmail(string customerName, string email);
    string RenderPasswordReset(string customerName, string resetUrl);
}

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<ApiResponse<NotificationDto>> SendCustomEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<NotificationDto>> SendOrderConfirmationAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ApiResponse<NotificationDto>> SendShipmentDispatchedAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<NotificationDto>>> GetOutboxMessagesAsync(NotificationStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ResendFailedNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
