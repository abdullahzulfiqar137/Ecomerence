using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface IBackgroundJobService
{
    Task<ApiResponse<int>> ReleaseExpiredStockHoldsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> CleanupAbandonedGuestCartsAsync(int olderThanDays = 30, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> DeactivateExpiredCouponsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> SyncLowStockAlertsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> ProcessPendingOutboxNotificationsAsync(CancellationToken cancellationToken = default);
}
