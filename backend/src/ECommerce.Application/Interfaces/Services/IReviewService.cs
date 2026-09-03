using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces.Services;

public interface IReviewService
{
    // Public / Customer APIs
    Task<ApiResponse<PagedResult<ProductReviewDto>>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10, string? sortBy = "newest", CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductReviewDto>> SubmitReviewAsync(Guid userId, CreateProductReviewDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> VoteHelpfulAsync(Guid reviewId, CancellationToken cancellationToken = default);

    // Admin / Moderation APIs
    Task<ApiResponse<PagedResult<ProductReviewDto>>> GetReviewsForModerationAsync(ReviewStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductReviewDto>> ModerateReviewAsync(Guid reviewId, ModerateReviewDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductReviewDto>> AddAdminResponseAsync(Guid reviewId, AdminReviewResponseDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
}
