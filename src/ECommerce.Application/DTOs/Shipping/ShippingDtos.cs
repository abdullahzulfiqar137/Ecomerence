using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Shipping;

public class ShippingZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CountryCodes { get; set; } = string.Empty;
    public List<ShippingRateDto> Rates { get; set; } = new();
}

public class CreateShippingZoneDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string CountryCodes { get; set; } = string.Empty;
}

public class UpdateShippingZoneDto : CreateShippingZoneDto
{
}

public class ShippingMethodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? EstimatedDeliveryDays { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateShippingMethodDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? EstimatedDeliveryDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateShippingMethodDto : CreateShippingMethodDto
{
}

public class ShippingRateDto
{
    public Guid Id { get; set; }
    public Guid ShippingZoneId { get; set; }
    public string ShippingZoneName { get; set; } = string.Empty;
    public Guid ShippingMethodId { get; set; }
    public string ShippingMethodName { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public decimal RatePerKg { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    public string Currency { get; set; } = "USD";
}

public class CreateShippingRateDto
{
    public Guid ShippingZoneId { get; set; }
    public Guid ShippingMethodId { get; set; }
    public decimal BaseRate { get; set; }
    public decimal RatePerKg { get; set; } = 0m;
    public decimal? FreeShippingThreshold { get; set; }
    public string Currency { get; set; } = "USD";
}

public class ShippingEstimateRequestDto
{
    public string CountryCode { get; set; } = "US";
    public decimal SubtotalAmount { get; set; }
    public decimal TotalWeightKg { get; set; } = 0.5m;
}

public class ShippingEstimateResponseDto
{
    public Guid ShippingMethodId { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string? EstimatedDeliveryDays { get; set; }
    public decimal ShippingCost { get; set; }
    public bool IsFreeShipping { get; set; }
    public string Currency { get; set; } = "USD";
}

public class ShipmentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string? TrackingUrl { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? EstimatedDeliveryUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public decimal ShippingCost { get; set; }
    public string? Notes { get; set; }
    public List<TrackingEventDto> TrackingEvents { get; set; } = new();
}

public class CreateShipmentDto
{
    public Guid OrderId { get; set; }
    public string CarrierName { get; set; } = "Standard";
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public decimal ShippingCost { get; set; } = 0m;
    public DateTime? EstimatedDeliveryUtc { get; set; }
    public string? Notes { get; set; }
}

public class UpdateShipmentStatusDto
{
    public ShipmentStatus NewStatus { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}

public class AddTrackingEventDto
{
    public ShipmentStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TrackingEventDto
{
    public Guid Id { get; set; }
    public ShipmentStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventTimeUtc { get; set; }
}
