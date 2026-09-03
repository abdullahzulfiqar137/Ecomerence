using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Coupons;

public class CouponService : ICouponService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CouponService> _logger;

    public CouponService(ApplicationDbContext context, ILogger<CouponService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<CouponDto>>> GetCouponsAsync(int page = 1, int pageSize = 20, bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Coupons.AsQueryable();

        if (isActiveOnly.HasValue && isActiveOnly.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(c => c.IsActive && c.StartsAtUtc <= now && (!c.ExpiresAtUtc.HasValue || c.ExpiresAtUtc.Value >= now));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var coupons = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = coupons.Select(c => MapToDto(c)).ToList();
        var pagedResult = new PagedResult<CouponDto>(dtos, totalItems, page, pageSize);

        return ApiResponse<PagedResult<CouponDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<CouponDto>> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { id }, cancellationToken);
        if (coupon == null)
        {
            return ApiResponse<CouponDto>.FailureResponse("Not Found", "Coupon not found.");
        }

        return ApiResponse<CouponDto>.SuccessResponse(MapToDto(coupon));
    }

    public async Task<ApiResponse<CouponDto>> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode, cancellationToken);
        if (coupon == null)
        {
            return ApiResponse<CouponDto>.FailureResponse("Not Found", "Coupon with specified code not found.");
        }

        return ApiResponse<CouponDto>.SuccessResponse(MapToDto(coupon));
    }

    public async Task<ApiResponse<CouponDto>> CreateCouponAsync(CreateCouponDto request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var exists = await _context.Coupons.AnyAsync(c => c.Code == normalizedCode, cancellationToken);
        if (exists)
        {
            return ApiResponse<CouponDto>.FailureResponse("Conflict", $"Coupon code '{normalizedCode}' already exists.");
        }

        var coupon = new Coupon
        {
            Code = normalizedCode,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaximumDiscountAmount = request.MaximumDiscountAmount,
            StartsAtUtc = request.StartsAtUtc,
            ExpiresAtUtc = request.ExpiresAtUtc,
            UsageLimitTotal = request.UsageLimitTotal,
            UsageLimitPerCustomer = request.UsageLimitPerCustomer,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.Coupons.AddAsync(coupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created coupon {CouponCode} ({DiscountType} - {DiscountValue})",
            coupon.Code, coupon.DiscountType, coupon.DiscountValue);

        return ApiResponse<CouponDto>.SuccessResponse(MapToDto(coupon));
    }

    public async Task<ApiResponse<CouponDto>> UpdateCouponAsync(Guid id, UpdateCouponDto request, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { id }, cancellationToken);
        if (coupon == null)
        {
            return ApiResponse<CouponDto>.FailureResponse("Not Found", "Coupon not found.");
        }

        coupon.Description = request.Description;
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        coupon.MaximumDiscountAmount = request.MaximumDiscountAmount;
        coupon.StartsAtUtc = request.StartsAtUtc;
        coupon.ExpiresAtUtc = request.ExpiresAtUtc;
        coupon.UsageLimitTotal = request.UsageLimitTotal;
        coupon.UsageLimitPerCustomer = request.UsageLimitPerCustomer;
        coupon.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CouponDto>.SuccessResponse(MapToDto(coupon));
    }

    public async Task<ApiResponse<bool>> DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { id }, cancellationToken);
        if (coupon == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Coupon not found.");
        }

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Coupon deleted successfully.");
    }

    public async Task<ApiResponse<ValidateCouponResponseDto>> ValidateCouponAsync(Guid? userId, ValidateCouponRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode, cancellationToken);

        if (coupon == null)
        {
            return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Invalid Coupon", "Coupon code does not exist.");
        }

        if (!coupon.IsActive)
        {
            return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Invalid Coupon", "This coupon is currently inactive.");
        }

        var now = DateTime.UtcNow;
        if (now < coupon.StartsAtUtc)
        {
            return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Invalid Coupon", "This promotion has not started yet.");
        }

        if (coupon.ExpiresAtUtc.HasValue && now > coupon.ExpiresAtUtc.Value)
        {
            return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Expired Coupon", "This coupon has expired.");
        }

        if (coupon.MinimumOrderAmount.HasValue && request.OrderSubtotal < coupon.MinimumOrderAmount.Value)
        {
            return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Minimum Spend Not Met",
                $"Minimum order amount of ${coupon.MinimumOrderAmount.Value:F2} is required for this coupon.");
        }

        if (coupon.UsageLimitTotal.HasValue && coupon.CurrentUsageCount >= coupon.UsageLimitTotal.Value)
        {
            return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Usage Limit Reached", "This coupon has reached its total usage limit.");
        }

        if (userId.HasValue && coupon.UsageLimitPerCustomer.HasValue)
        {
            var userUsageCount = await _context.CouponUsages
                .CountAsync(u => u.CouponId == coupon.Id && u.UserId == userId.Value, cancellationToken);

            if (userUsageCount >= coupon.UsageLimitPerCustomer.Value)
            {
                return ApiResponse<ValidateCouponResponseDto>.FailureResponse("Usage Limit Reached",
                    $"You have already used this coupon the maximum allowed times ({coupon.UsageLimitPerCustomer.Value}).");
            }
        }

        // Calculate Discount
        decimal discountAmount;
        if (coupon.DiscountType == DiscountType.Percentage)
        {
            discountAmount = Math.Round(request.OrderSubtotal * (coupon.DiscountValue / 100m), 2);
            if (coupon.MaximumDiscountAmount.HasValue && discountAmount > coupon.MaximumDiscountAmount.Value)
            {
                discountAmount = coupon.MaximumDiscountAmount.Value;
            }
        }
        else
        {
            discountAmount = Math.Min(request.OrderSubtotal, coupon.DiscountValue);
        }

        var newSubtotal = Math.Max(0, request.OrderSubtotal - discountAmount);

        var result = new ValidateCouponResponseDto
        {
            IsValid = true,
            Message = "Coupon is valid and applied.",
            Code = coupon.Code,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            CalculatedDiscountAmount = discountAmount,
            NewSubtotal = newSubtotal
        };

        return ApiResponse<ValidateCouponResponseDto>.SuccessResponse(result, "Coupon validated successfully.");
    }

    public async Task<ApiResponse<bool>> RecordUsageAsync(string couponCode, Guid orderId, Guid? userId, decimal discountApplied, CancellationToken cancellationToken = default)
    {
        var normalizedCode = couponCode.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode, cancellationToken);
        if (coupon == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Coupon not found.");
        }

        var usage = new CouponUsage
        {
            CouponId = coupon.Id,
            OrderId = orderId,
            UserId = userId,
            DiscountApplied = discountApplied,
            UsedAtUtc = DateTime.UtcNow
        };

        coupon.CurrentUsageCount++;

        await _context.CouponUsages.AddAsync(usage, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded coupon usage for {CouponCode} on Order {OrderId} (-${Discount})",
            coupon.Code, orderId, discountApplied);

        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<IReadOnlyList<CouponUsageDto>>> GetCouponUsagesAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var usages = await _context.CouponUsages
            .Include(u => u.Order)
            .Include(u => u.User)
            .Where(u => u.CouponId == couponId)
            .OrderByDescending(u => u.UsedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = usages.Select(u => new CouponUsageDto
        {
            Id = u.Id,
            CouponId = u.CouponId,
            OrderId = u.OrderId,
            OrderNumber = u.Order?.OrderNumber ?? string.Empty,
            UserId = u.UserId,
            UserEmail = u.User?.Email,
            DiscountApplied = u.DiscountApplied,
            UsedAtUtc = u.UsedAtUtc
        }).ToList();

        return ApiResponse<IReadOnlyList<CouponUsageDto>>.SuccessResponse(dtos);
    }

    private static CouponDto MapToDto(Coupon c)
    {
        return new CouponDto
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MinimumOrderAmount = c.MinimumOrderAmount,
            MaximumDiscountAmount = c.MaximumDiscountAmount,
            StartsAtUtc = c.StartsAtUtc,
            ExpiresAtUtc = c.ExpiresAtUtc,
            UsageLimitTotal = c.UsageLimitTotal,
            UsageLimitPerCustomer = c.UsageLimitPerCustomer,
            CurrentUsageCount = c.CurrentUsageCount,
            IsActive = c.IsActive,
            CreatedAtUtc = c.CreatedAtUtc
        };
    }
}
