namespace ECommerce.Domain.Enums;

public enum NotificationType
{
    OrderPlaced = 1,
    OrderConfirmed = 2,
    OrderShipped = 3,
    OrderDelivered = 4,
    OrderCancelled = 5,
    PaymentCaptured = 6,
    PaymentRefunded = 7,
    WelcomeCustomer = 8,
    PasswordReset = 9,
    LowStockAlert = 10
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}
