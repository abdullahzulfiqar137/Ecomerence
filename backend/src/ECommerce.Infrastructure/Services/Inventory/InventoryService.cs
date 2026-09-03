using ECommerce.Application.DTOs.Inventory;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        ApplicationDbContext context,
        ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<InventoryItemDto>>> GetInventoryAsync(InventoryFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryItems
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .AsQueryable();

        if (filterParams.WarehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == filterParams.WarehouseId.Value);
        }

        if (filterParams.ProductId.HasValue)
        {
            query = query.Where(i => i.ProductId == filterParams.ProductId.Value);
        }

        if (filterParams.LowStockOnly == true)
        {
            query = query.Where(i => (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderPoint);
        }

        if (!string.IsNullOrEmpty(filterParams.SearchTerm))
        {
            query = query.Where(i =>
                i.Product!.Name.Contains(filterParams.SearchTerm) ||
                i.Product.SKU.Contains(filterParams.SearchTerm) ||
                (i.ProductVariant != null && i.ProductVariant.SKU.Contains(filterParams.SearchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Warehouse!.Name)
            .ThenBy(i => i.Product!.Name)
            .Skip((filterParams.Page - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(i => MapToDto(i)).ToList();
        var pagedResult = new PagedResult<InventoryItemDto>(dtos, totalCount, filterParams.Page, filterParams.PageSize);

        return ApiResponse<PagedResult<InventoryItemDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<InventoryItemDto>> GetStockByWarehouseAndProductAsync(Guid warehouseId, Guid productId, Guid? variantId = null, CancellationToken cancellationToken = default)
    {
        var item = await _context.InventoryItems
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .FirstOrDefaultAsync(i => i.WarehouseId == warehouseId && i.ProductId == productId && i.ProductVariantId == variantId, cancellationToken);

        if (item == null)
        {
            return ApiResponse<InventoryItemDto>.FailureResponse("Not Found", "Inventory item not found for specified warehouse and product/variant.");
        }

        return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(item));
    }

    public async Task<ApiResponse<InventoryItemDto>> AdjustStockAsync(AdjustStockRequestDto request, string? username = null, CancellationToken cancellationToken = default)
    {
        var item = await _context.InventoryItems
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .FirstOrDefaultAsync(i => i.WarehouseId == request.WarehouseId && i.ProductId == request.ProductId && i.ProductVariantId == request.ProductVariantId, cancellationToken);

        if (item == null)
        {
            // Auto-create inventory record for warehouse/product if not already registered
            item = new InventoryItem
            {
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                ProductVariantId = request.ProductVariantId,
                QuantityOnHand = 0,
                QuantityReserved = 0
            };
            await _context.InventoryItems.AddAsync(item, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var oldQuantity = item.QuantityOnHand;
        var diff = request.NewQuantity - oldQuantity;

        if (diff == 0)
        {
            return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(item), "Quantity unchanged.");
        }

        item.QuantityOnHand = request.NewQuantity;

        // 1. Audit Adjustment record
        var adjustment = new StockAdjustment
        {
            InventoryItemId = item.Id,
            OldQuantity = oldQuantity,
            NewQuantity = request.NewQuantity,
            Reason = request.Reason,
            ApprovedBy = username ?? "System",
            ApprovedAtUtc = DateTime.UtcNow
        };
        await _context.StockAdjustments.AddAsync(adjustment, cancellationToken);

        // 2. Ledger Movement record
        var movement = new StockMovement
        {
            InventoryItemId = item.Id,
            Type = StockMovementType.ManualAdjustment,
            Quantity = diff,
            QuantityBefore = oldQuantity,
            QuantityAfter = request.NewQuantity,
            ReferenceType = "StockAdjustment",
            ReferenceId = adjustment.Id.ToString(),
            Notes = request.Reason,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = username ?? "System"
        };
        await _context.StockMovements.AddAsync(movement, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Adjusted stock for Item {ItemId}: {Old} -> {New} ({Reason})", item.Id, oldQuantity, request.NewQuantity, request.Reason);

        return ApiResponse<InventoryItemDto>.SuccessResponse(MapToDto(item), "Stock adjusted successfully.");
    }

    public async Task<ApiResponse<InventoryReservationDto>> ReserveStockAsync(CreateReservationRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.WarehouseId == request.WarehouseId && i.ProductId == request.ProductId && i.ProductVariantId == request.ProductVariantId, cancellationToken);

        if (item == null)
        {
            return ApiResponse<InventoryReservationDto>.FailureResponse("Not Found", "Inventory item not found.");
        }

        // Check availability
        if (item.QuantityAvailable < request.Quantity)
        {
            return ApiResponse<InventoryReservationDto>.FailureResponse("Insufficient Stock", $"Requested quantity {request.Quantity} exceeds available stock {item.QuantityAvailable}.");
        }

        item.QuantityReserved += request.Quantity;

        var reservation = new InventoryReservation
        {
            InventoryItemId = item.Id,
            OrderId = request.OrderId,
            CartSessionId = request.CartSessionId,
            Quantity = request.Quantity,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(request.HoldDurationMinutes),
            CreatedAtUtc = DateTime.UtcNow,
            IsReleased = false,
            IsCommitted = false
        };

        await _context.InventoryReservations.AddAsync(reservation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reserved {Qty} units of Item {ItemId} until {Expires}", request.Quantity, item.Id, reservation.ExpiresAtUtc);

        var dto = new InventoryReservationDto
        {
            Id = reservation.Id,
            InventoryItemId = reservation.InventoryItemId,
            OrderId = reservation.OrderId,
            CartSessionId = reservation.CartSessionId,
            Quantity = reservation.Quantity,
            ExpiresAtUtc = reservation.ExpiresAtUtc,
            IsReleased = reservation.IsReleased,
            IsCommitted = reservation.IsCommitted
        };

        return ApiResponse<InventoryReservationDto>.SuccessResponse(dto, "Stock reserved successfully.");
    }

    public async Task<ApiResponse<bool>> ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.InventoryReservations
            .Include(r => r.InventoryItem)
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

        if (reservation == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Reservation not found.");
        }

        if (reservation.IsReleased || reservation.IsCommitted)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Reservation was already processed.");
        }

        if (reservation.InventoryItem != null)
        {
            reservation.InventoryItem.QuantityReserved = Math.Max(0, reservation.InventoryItem.QuantityReserved - reservation.Quantity);
        }

        reservation.IsReleased = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Released reservation {ResId} ({Qty} units returned to available)", reservationId, reservation.Quantity);

        return ApiResponse<bool>.SuccessResponse(true, "Reservation released successfully.");
    }

    public async Task<ApiResponse<bool>> CommitReservationAsync(Guid reservationId, string? orderNumber = null, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.InventoryReservations
            .Include(r => r.InventoryItem)
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

        if (reservation == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Reservation not found.");
        }

        if (reservation.IsCommitted)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Reservation already committed.");
        }

        var item = reservation.InventoryItem;
        if (item != null)
        {
            var qtyBefore = item.QuantityOnHand;
            item.QuantityOnHand = Math.Max(0, item.QuantityOnHand - reservation.Quantity);
            item.QuantityReserved = Math.Max(0, item.QuantityReserved - reservation.Quantity);

            // Log movement
            var movement = new StockMovement
            {
                InventoryItemId = item.Id,
                Type = StockMovementType.OrderOutflow,
                Quantity = -reservation.Quantity,
                QuantityBefore = qtyBefore,
                QuantityAfter = item.QuantityOnHand,
                ReferenceType = "Order",
                ReferenceId = orderNumber ?? reservation.OrderId?.ToString(),
                Notes = "Committed checkout reservation",
                CreatedAtUtc = DateTime.UtcNow
            };
            await _context.StockMovements.AddAsync(movement, cancellationToken);
        }

        reservation.IsCommitted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Committed reservation {ResId} ({Qty} units permanently deducted)", reservationId, reservation.Quantity);

        return ApiResponse<bool>.SuccessResponse(true, "Reservation committed successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<LowStockAlertDto>>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default)
    {
        var lowStockItems = await _context.InventoryItems
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .Where(i => (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderPoint)
            .ToListAsync(cancellationToken);

        var alerts = lowStockItems.Select(i => new LowStockAlertDto
        {
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            ProductSKU = i.Product?.SKU ?? string.Empty,
            ProductVariantId = i.ProductVariantId,
            VariantName = i.ProductVariant?.Name,
            VariantSKU = i.ProductVariant?.SKU,
            WarehouseId = i.WarehouseId,
            WarehouseName = i.Warehouse?.Name ?? string.Empty,
            QuantityAvailable = i.QuantityAvailable,
            ReorderPoint = i.ReorderPoint,
            ReorderQuantity = i.ReorderQuantity
        }).ToList();

        return ApiResponse<IReadOnlyList<LowStockAlertDto>>.SuccessResponse(alerts);
    }

    public async Task<ApiResponse<IReadOnlyList<StockMovementDto>>> GetStockMovementsAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var movements = await _context.StockMovements
            .Where(m => m.InventoryItemId == inventoryItemId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        var dtos = movements.Select(m => new StockMovementDto
        {
            Id = m.Id,
            InventoryItemId = m.InventoryItemId,
            Type = m.Type,
            Quantity = m.Quantity,
            QuantityBefore = m.QuantityBefore,
            QuantityAfter = m.QuantityAfter,
            ReferenceType = m.ReferenceType,
            ReferenceId = m.ReferenceId,
            Notes = m.Notes,
            CreatedAtUtc = m.CreatedAtUtc,
            CreatedBy = m.CreatedBy
        }).ToList();

        return ApiResponse<IReadOnlyList<StockMovementDto>>.SuccessResponse(dtos);
    }

    private static InventoryItemDto MapToDto(InventoryItem i)
    {
        return new InventoryItemDto
        {
            Id = i.Id,
            WarehouseId = i.WarehouseId,
            WarehouseName = i.Warehouse?.Name ?? string.Empty,
            WarehouseCode = i.Warehouse?.Code ?? string.Empty,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            ProductSKU = i.Product?.SKU ?? string.Empty,
            ProductVariantId = i.ProductVariantId,
            VariantName = i.ProductVariant?.Name,
            VariantSKU = i.ProductVariant?.SKU,
            QuantityOnHand = i.QuantityOnHand,
            QuantityReserved = i.QuantityReserved,
            QuantityAvailable = i.QuantityAvailable,
            ReorderPoint = i.ReorderPoint,
            ReorderQuantity = i.ReorderQuantity
        };
    }
}
