namespace ECommerce.Application.DTOs.Cart;

public class CartDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? GuestSessionId { get; set; }
    public string Currency { get; set; } = "USD";
    public string? AppliedCouponCode { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount => Math.Max(0, SubtotalAmount - DiscountAmount);
    public int TotalItems => Items.Where(i => !i.IsSavedForLater).Sum(i => i.Quantity);
    public List<CartItemDto> Items { get; set; } = new();
    public List<CartItemDto> SavedForLaterItems { get; set; } = new();
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? ProductVariantId { get; set; }
    public string? VariantName { get; set; }
    public string? VariantSKU { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public bool IsInStock { get; set; }
    public int MaxAvailableStock { get; set; }
    public bool IsSavedForLater { get; set; }
}

public class AddToCartRequestDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? GuestSessionId { get; set; }
}

public class UpdateCartItemRequestDto
{
    public int Quantity { get; set; }
}

public class MergeCartRequestDto
{
    public string GuestSessionId { get; set; } = string.Empty;
}

public class ApplyCartCouponRequestDto
{
    public string CouponCode { get; set; } = string.Empty;
    public string? GuestSessionId { get; set; }
}
