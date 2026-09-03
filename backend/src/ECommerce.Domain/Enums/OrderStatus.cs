namespace ECommerce.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Packed = 4,
    Shipped = 5,
    OutForDelivery = 6,
    Delivered = 7,
    Cancelled = 8,
    Returned = 9,
    Refunded = 10,
    Failed = 11
}
