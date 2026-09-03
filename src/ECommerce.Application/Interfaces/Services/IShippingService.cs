using ECommerce.Application.DTOs.Shipping;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

public interface IShippingService
{
    // Zone & Method Management
    Task<ApiResponse<IReadOnlyList<ShippingZoneDto>>> GetAllZonesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ShippingZoneDto>> CreateZoneAsync(CreateShippingZoneDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<ShippingMethodDto>>> GetAllMethodsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ShippingMethodDto>> CreateMethodAsync(CreateShippingMethodDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShippingRateDto>> CreateRateAsync(CreateShippingRateDto request, CancellationToken cancellationToken = default);

    // Rate Estimation
    Task<ApiResponse<IReadOnlyList<ShippingEstimateResponseDto>>> EstimateShippingRatesAsync(ShippingEstimateRequestDto request, CancellationToken cancellationToken = default);

    // Shipment & Tracking Lifecycle
    Task<ApiResponse<ShipmentDto>> CreateShipmentAsync(CreateShipmentDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShipmentDto>> GetShipmentByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShipmentDto>> GetShipmentByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShipmentDto>> AddTrackingEventAsync(Guid shipmentId, AddTrackingEventDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShipmentDto>> UpdateShipmentStatusAsync(Guid shipmentId, UpdateShipmentStatusDto request, CancellationToken cancellationToken = default);
}
