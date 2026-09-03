using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ICouponService
{
    Task<ApiResponse<PagedResult<CouponDto>>> GetCouponsAsync(int page = 1, int pageSize = 20, bool? isActiveOnly = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CouponDto>> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CouponDto>> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ApiResponse<CouponDto>> CreateCouponAsync(CreateCouponDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CouponDto>> UpdateCouponAsync(Guid id, UpdateCouponDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<ValidateCouponResponseDto>> ValidateCouponAsync(Guid? userId, ValidateCouponRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RecordUsageAsync(string couponCode, Guid orderId, Guid? userId, decimal discountApplied, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<CouponUsageDto>>> GetCouponUsagesAsync(Guid couponId, CancellationToken cancellationToken = default);
}
