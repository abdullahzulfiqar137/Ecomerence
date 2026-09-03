using ECommerce.Application.DTOs.Shipping;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Shipping;

public class ShippingService : IShippingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(ApplicationDbContext context, ILogger<ShippingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<ShippingZoneDto>>> GetAllZonesAsync(CancellationToken cancellationToken = default)
    {
        var zones = await _context.ShippingZones
            .Include(z => z.ShippingRates)
                .ThenInclude(r => r.ShippingMethod)
            .OrderBy(z => z.Name)
            .ToListAsync(cancellationToken);

        var dtos = zones.Select(z => MapToZoneDto(z)).ToList();
        return ApiResponse<IReadOnlyList<ShippingZoneDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<ShippingZoneDto>> CreateZoneAsync(CreateShippingZoneDto request, CancellationToken cancellationToken = default)
    {
        var zone = new ShippingZone
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            CountryCodes = request.CountryCodes.Trim().ToUpperInvariant(),
            IsActive = request.IsActive
        };

        await _context.ShippingZones.AddAsync(zone, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created shipping zone {ZoneName} for countries: {Countries}", zone.Name, zone.CountryCodes);

        return ApiResponse<ShippingZoneDto>.SuccessResponse(MapToZoneDto(zone));
    }

    public async Task<ApiResponse<IReadOnlyList<ShippingMethodDto>>> GetAllMethodsAsync(CancellationToken cancellationToken = default)
    {
        var methods = await _context.ShippingMethods
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);

        var dtos = methods.Select(m => new ShippingMethodDto
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            Description = m.Description,
            EstimatedDeliveryDays = m.EstimatedDeliveryDays,
            IsActive = m.IsActive,
            DisplayOrder = m.DisplayOrder
        }).ToList();

        return ApiResponse<IReadOnlyList<ShippingMethodDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<ShippingMethodDto>> CreateMethodAsync(CreateShippingMethodDto request, CancellationToken cancellationToken = default)
    {
        var method = new ShippingMethod
        {
            Name = request.Name.Trim(),
            Code = request.Code?.Trim().ToUpperInvariant(),
            Description = request.Description,
            EstimatedDeliveryDays = request.EstimatedDeliveryDays,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        await _context.ShippingMethods.AddAsync(method, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ShippingMethodDto>.SuccessResponse(new ShippingMethodDto
        {
            Id = method.Id,
            Name = method.Name,
            Code = method.Code,
            Description = method.Description,
            EstimatedDeliveryDays = method.EstimatedDeliveryDays,
            IsActive = method.IsActive,
            DisplayOrder = method.DisplayOrder
        });
    }

    public async Task<ApiResponse<ShippingRateDto>> CreateRateAsync(CreateShippingRateDto request, CancellationToken cancellationToken = default)
    {
        var zone = await _context.ShippingZones.FindAsync(new object[] { request.ShippingZoneId }, cancellationToken);
        if (zone == null)
        {
            return ApiResponse<ShippingRateDto>.FailureResponse("Not Found", "Shipping zone not found.");
        }

        var method = await _context.ShippingMethods.FindAsync(new object[] { request.ShippingMethodId }, cancellationToken);
        if (method == null)
        {
            return ApiResponse<ShippingRateDto>.FailureResponse("Not Found", "Shipping method not found.");
        }

        var rate = new ShippingRate
        {
            ShippingZoneId = request.ShippingZoneId,
            ShippingMethodId = request.ShippingMethodId,
            BaseRate = request.BaseRate,
            RatePerKg = request.RatePerKg,
            FreeShippingThreshold = request.FreeShippingThreshold,
            Currency = request.Currency
        };

        await _context.ShippingRates.AddAsync(rate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ShippingRateDto>.SuccessResponse(new ShippingRateDto
        {
            Id = rate.Id,
            ShippingZoneId = rate.ShippingZoneId,
            ShippingZoneName = zone.Name,
            ShippingMethodId = rate.ShippingMethodId,
            ShippingMethodName = method.Name,
            BaseRate = rate.BaseRate,
            RatePerKg = rate.RatePerKg,
            FreeShippingThreshold = rate.FreeShippingThreshold,
            Currency = rate.Currency
        });
    }

    public async Task<ApiResponse<IReadOnlyList<ShippingEstimateResponseDto>>> EstimateShippingRatesAsync(ShippingEstimateRequestDto request, CancellationToken cancellationToken = default)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();

        // 1. Find matching zone
        var zones = await _context.ShippingZones
            .Include(z => z.ShippingRates)
                .ThenInclude(r => r.ShippingMethod)
            .Where(z => z.IsActive)
            .ToListAsync(cancellationToken);

        var matchingZone = zones.FirstOrDefault(z =>
            z.CountryCodes.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Contains(countryCode));

        var estimates = new List<ShippingEstimateResponseDto>();

        if (matchingZone != null && matchingZone.ShippingRates.Any())
        {
            var methodIds = matchingZone.ShippingRates.Select(r => r.ShippingMethodId).Distinct().ToList();
            var methodsDict = await _context.ShippingMethods
                .Where(m => methodIds.Contains(m.Id) && m.IsActive)
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            foreach (var rate in matchingZone.ShippingRates)
            {
                if (!methodsDict.TryGetValue(rate.ShippingMethodId, out var method))
                {
                    continue;
                }

                var isFree = rate.FreeShippingThreshold.HasValue && request.SubtotalAmount >= rate.FreeShippingThreshold.Value;
                var cost = isFree ? 0m : Math.Round(rate.BaseRate + (request.TotalWeightKg * rate.RatePerKg), 2);

                estimates.Add(new ShippingEstimateResponseDto
                {
                    ShippingMethodId = rate.ShippingMethodId,
                    MethodName = method.Name,
                    EstimatedDeliveryDays = method.EstimatedDeliveryDays,
                    ShippingCost = cost,
                    IsFreeShipping = isFree,
                    Currency = rate.Currency
                });
            }
        }
        else
        {
            // Fallback standard rates
            var methods = await _context.ShippingMethods.Where(m => m.IsActive).ToListAsync(cancellationToken);
            foreach (var m in methods)
            {
                var cost = m.Name.Contains("Express", StringComparison.OrdinalIgnoreCase) ? 15.00m : 5.00m;
                estimates.Add(new ShippingEstimateResponseDto
                {
                    ShippingMethodId = m.Id,
                    MethodName = m.Name,
                    EstimatedDeliveryDays = m.EstimatedDeliveryDays,
                    ShippingCost = cost,
                    IsFreeShipping = false,
                    Currency = "USD"
                });
            }
        }

        return ApiResponse<IReadOnlyList<ShippingEstimateResponseDto>>.SuccessResponse(estimates);
    }

    public async Task<ApiResponse<ShipmentDto>> CreateShipmentAsync(CreateShipmentDto request, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return ApiResponse<ShipmentDto>.FailureResponse("Not Found", "Order not found.");
        }

        var carrier = request.CarrierName.Trim();
        var trackingNumber = !string.IsNullOrWhiteSpace(request.TrackingNumber)
            ? request.TrackingNumber.Trim()
            : GenerateTrackingNumber(carrier);

        var shipment = new Shipment
        {
            OrderId = order.Id,
            CarrierName = carrier,
            TrackingNumber = trackingNumber,
            TrackingUrl = request.TrackingUrl ?? $"https://track.carrier.com/{trackingNumber}",
            Status = ShipmentStatus.InTransit,
            ShippedAtUtc = DateTime.UtcNow,
            EstimatedDeliveryUtc = request.EstimatedDeliveryUtc ?? DateTime.UtcNow.AddDays(3),
            ShippingCost = request.ShippingCost,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Add initial tracking event
        shipment.TrackingEvents.Add(new TrackingEvent
        {
            Shipment = shipment,
            Status = ShipmentStatus.InTransit,
            Location = "Fulfillment Hub",
            Description = $"Package dispatched via {carrier} with tracking {trackingNumber}",
            EventTimeUtc = DateTime.UtcNow
        });

        // Add shipment items for each order item
        foreach (var item in order.Items)
        {
            shipment.Items.Add(new ShipmentItem
            {
                Shipment = shipment,
                OrderItemId = item.Id,
                Quantity = item.Quantity
            });
        }

        // Update Order shipping status
        order.ShippingStatus = ShipmentStatus.InTransit;
        if (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Processing || order.Status == OrderStatus.Packed)
        {
            order.Status = OrderStatus.Shipped;
        }

        await _context.Shipments.AddAsync(shipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created shipment {TrackingNumber} for Order {OrderNumber} via {Carrier}",
            trackingNumber, order.OrderNumber, carrier);

        return await GetShipmentByIdAsync(shipment.Id, cancellationToken);
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(s => s.Id == shipmentId, cancellationToken);

        if (shipment == null)
        {
            return ApiResponse<ShipmentDto>.FailureResponse("Not Found", "Shipment not found.");
        }

        return ApiResponse<ShipmentDto>.SuccessResponse(MapToShipmentDto(shipment));
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber.Trim(), cancellationToken);

        if (shipment == null)
        {
            return ApiResponse<ShipmentDto>.FailureResponse("Not Found", "Shipment with specified tracking number not found.");
        }

        return ApiResponse<ShipmentDto>.SuccessResponse(MapToShipmentDto(shipment));
    }

    public async Task<ApiResponse<ShipmentDto>> AddTrackingEventAsync(Guid shipmentId, AddTrackingEventDto request, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(s => s.Id == shipmentId, cancellationToken);

        if (shipment == null)
        {
            return ApiResponse<ShipmentDto>.FailureResponse("Not Found", "Shipment not found.");
        }

        var trackingEvent = new TrackingEvent
        {
            ShipmentId = shipment.Id,
            Status = request.Status,
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            EventTimeUtc = DateTime.UtcNow
        };

        shipment.Status = request.Status;
        if (request.Status == ShipmentStatus.Delivered)
        {
            shipment.DeliveredAtUtc = DateTime.UtcNow;
            if (shipment.Order != null)
            {
                shipment.Order.ShippingStatus = ShipmentStatus.Delivered;
                shipment.Order.Status = OrderStatus.Delivered;
            }
        }

        await _context.TrackingEvents.AddAsync(trackingEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added tracking event '{Description}' at {Location} for Shipment {TrackingNumber}",
            request.Description, request.Location, shipment.TrackingNumber);

        return await GetShipmentByIdAsync(shipment.Id, cancellationToken);
    }

    public async Task<ApiResponse<ShipmentDto>> UpdateShipmentStatusAsync(Guid shipmentId, UpdateShipmentStatusDto request, CancellationToken cancellationToken = default)
    {
        return await AddTrackingEventAsync(shipmentId, new AddTrackingEventDto
        {
            Status = request.NewStatus,
            Location = request.Location ?? "Transit Hub",
            Description = request.Description ?? $"Shipment status updated to {request.NewStatus}"
        }, cancellationToken);
    }

    private static string GenerateTrackingNumber(string carrier)
    {
        var prefix = carrier.ToUpperInvariant() switch
        {
            "TCS" => "TCS",
            "LEOPARDS" => "LPD",
            "DHL" => "DHL",
            "FEDEX" => "FDX",
            _ => "TRK"
        };
        return $"{prefix}{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100000, 999999)}";
    }

    private static ShippingZoneDto MapToZoneDto(ShippingZone z)
    {
        return new ShippingZoneDto
        {
            Id = z.Id,
            Name = z.Name,
            Description = z.Description,
            IsActive = z.IsActive,
            CountryCodes = z.CountryCodes,
            Rates = z.ShippingRates.Select(r => new ShippingRateDto
            {
                Id = r.Id,
                ShippingZoneId = r.ShippingZoneId,
                ShippingZoneName = z.Name,
                ShippingMethodId = r.ShippingMethodId,
                ShippingMethodName = r.ShippingMethod?.Name ?? string.Empty,
                BaseRate = r.BaseRate,
                RatePerKg = r.RatePerKg,
                FreeShippingThreshold = r.FreeShippingThreshold,
                Currency = r.Currency
            }).ToList()
        };
    }

    private static ShipmentDto MapToShipmentDto(Shipment s)
    {
        return new ShipmentDto
        {
            Id = s.Id,
            OrderId = s.OrderId,
            OrderNumber = s.Order?.OrderNumber ?? string.Empty,
            CarrierName = s.CarrierName,
            TrackingNumber = s.TrackingNumber,
            TrackingUrl = s.TrackingUrl,
            Status = s.Status,
            ShippedAtUtc = s.ShippedAtUtc,
            EstimatedDeliveryUtc = s.EstimatedDeliveryUtc,
            DeliveredAtUtc = s.DeliveredAtUtc,
            ShippingCost = s.ShippingCost,
            Notes = s.Notes,
            TrackingEvents = s.TrackingEvents.OrderByDescending(e => e.EventTimeUtc).Select(e => new TrackingEventDto
            {
                Id = e.Id,
                Status = e.Status,
                Location = e.Location,
                Description = e.Description,
                EventTimeUtc = e.EventTimeUtc
            }).ToList()
        };
    }
}
