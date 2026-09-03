using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Notification : AuditableEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;

    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime? SentAtUtc { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAtUtc { get; set; }

    public Guid? ReferenceId { get; set; } // OrderId, PaymentId, ProductId
    public string? ReferenceType { get; set; }
}
