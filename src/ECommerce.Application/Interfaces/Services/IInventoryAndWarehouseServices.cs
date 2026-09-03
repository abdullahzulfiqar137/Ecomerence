using ECommerce.Application.DTOs.Inventory;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface IWarehouseService
{
    Task<ApiResponse<IReadOnlyList<WarehouseDto>>> GetAllWarehousesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid id, UpdateWarehouseDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IInventoryService
{
    Task<ApiResponse<PagedResult<InventoryItemDto>>> GetInventoryAsync(InventoryFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryItemDto>> GetStockByWarehouseAndProductAsync(Guid warehouseId, Guid productId, Guid? variantId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryItemDto>> AdjustStockAsync(AdjustStockRequestDto request, string? username = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryReservationDto>> ReserveStockAsync(CreateReservationRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> CommitReservationAsync(Guid reservationId, string? orderNumber = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<LowStockAlertDto>>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<StockMovementDto>>> GetStockMovementsAsync(Guid inventoryItemId, CancellationToken cancellationToken = default);
}
