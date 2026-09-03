using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Analytics;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<ApiResponse<DashboardKpiDto>> GetDashboardKpiAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<SalesTrendDataPointDto>>> GetSalesTrendAsync(SalesTrendRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<TopSellingProductDto>>> GetTopSellingProductsAsync(int top = 10, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<OrderStatusDistributionDto>>> GetOrderStatusDistributionAsync(CancellationToken cancellationToken = default);
}

public interface IAdminUserService
{
    Task<ApiResponse<PagedResult<AdminUserListDto>>> GetUsersAsync(string? search = null, string? role = null, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminUserDetailsDto>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminUserDetailsDto>> AssignRolesAsync(Guid userId, AssignUserRolesDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminUserDetailsDto>> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto request, CancellationToken cancellationToken = default);
}
