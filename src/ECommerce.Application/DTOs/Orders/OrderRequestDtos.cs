using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders;

public class CreateOrderDto
{
    public string? IdempotencyKey { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string? AppliedCouponCode { get; set; }
    public string? Notes { get; set; }

    public CreateOrderAddressDto ShippingAddress { get; set; } = null!;
    public CreateOrderAddressDto? BillingAddress { get; set; }

    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderAddressDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? PhoneNumber { get; set; }
    public string StreetLine1 { get; set; } = string.Empty;
    public string? StreetLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class UpdateOrderStatusDto
{
    public OrderStatus NewStatus { get; set; }
    public string? Comment { get; set; }
}

public class AddOrderNoteDto
{
    public string Note { get; set; } = string.Empty;
    public bool IsCustomerVisible { get; set; } = false;
}

public class CancelOrderRequestDto
{
    public string Reason { get; set; } = string.Empty;
}
