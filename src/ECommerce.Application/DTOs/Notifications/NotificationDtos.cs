using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class SendEmailRequestDto
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.WelcomeCustomer;
    public Guid? UserId { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}

public class SendOrderConfirmationEmailDto
{
    public Guid OrderId { get; set; }
}

public class SendShippingNotificationEmailDto
{
    public Guid ShipmentId { get; set; }
}
