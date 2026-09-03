using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Coupons;

public class CouponDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerCustomer { get; set; }
    public int CurrentUsageCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerCustomer { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class UpdateCouponDto
{
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerCustomer { get; set; }
    public bool IsActive { get; set; }
}

public class ValidateCouponRequestDto
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderSubtotal { get; set; }
}

public class ValidateCouponResponseDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal CalculatedDiscountAmount { get; set; }
    public decimal NewSubtotal { get; set; }
}

public class CouponUsageDto
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime UsedAtUtc { get; set; }
}
