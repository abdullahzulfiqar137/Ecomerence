using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Reviews;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ProductReviewDto>>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10, string? sortBy = "newest", CancellationToken cancellationToken = default)
    {
        var query = _context.ProductReviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .AsQueryable();

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "highest_rating" or "rating_desc" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAtUtc),
            "lowest_rating" or "rating_asc" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAtUtc),
            "most_helpful" => query.OrderByDescending(r => r.HelpfulVotesCount).ThenByDescending(r => r.CreatedAtUtc),
            _ => query.OrderByDescending(r => r.CreatedAtUtc)
        };

        var total = await query.CountAsync(cancellationToken);
        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = reviews.Select(r => MapToDto(r)).ToList();
        var pagedResult = new PagedResult<ProductReviewDto>(dtos, total, page, pageSize);

        return ApiResponse<PagedResult<ProductReviewDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var approvedReviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .ToListAsync(cancellationToken);

        var total = approvedReviews.Count;
        var avg = total > 0 ? Math.Round((decimal)approvedReviews.Average(r => r.Rating), 2) : 0m;

        var summary = new ProductReviewSummaryDto
        {
            ProductId = productId,
            AverageRating = avg,
            TotalReviews = total,
            FiveStarCount = approvedReviews.Count(r => r.Rating == 5),
            FourStarCount = approvedReviews.Count(r => r.Rating == 4),
            ThreeStarCount = approvedReviews.Count(r => r.Rating == 3),
            TwoStarCount = approvedReviews.Count(r => r.Rating == 2),
            OneStarCount = approvedReviews.Count(r => r.Rating == 1)
        };

        return ApiResponse<ProductReviewSummaryDto>.SuccessResponse(summary);
    }

    public async Task<ApiResponse<ProductReviewDto>> SubmitReviewAsync(Guid userId, CreateProductReviewDto request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
        if (product == null)
        {
            return ApiResponse<ProductReviewDto>.FailureResponse("Not Found", "Product not found.");
        }

        var existing = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.ProductId == request.ProductId && r.UserId == userId, cancellationToken);

        if (existing != null)
        {
            return ApiResponse<ProductReviewDto>.FailureResponse("Conflict", "You have already submitted a review for this product.");
        }

        // Check verified purchase (user ordered this product and order is confirmed/shipped/delivered)
        var isVerified = await _context.OrderItems
            .AnyAsync(oi => oi.ProductId == request.ProductId &&
                            oi.Order != null &&
                            oi.Order.UserId == userId &&
                            (oi.Order.Status == OrderStatus.Delivered ||
                             oi.Order.Status == OrderStatus.Shipped ||
                             oi.Order.Status == OrderStatus.Confirmed), cancellationToken);

        var review = new ProductReview
        {
            ProductId = request.ProductId,
            UserId = userId,
            Rating = request.Rating,
            Title = request.Title.Trim(),
            Comment = request.Comment.Trim(),
            IsVerifiedPurchase = isVerified,
            Status = ReviewStatus.PendingModeration,
            HelpfulVotesCount = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.ProductReviews.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} submitted a review for Product {ProductId} (Rating: {Rating}, Verified: {Verified})",
            userId, request.ProductId, request.Rating, isVerified);

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        review.Product = product;
        review.User = user;

        return ApiResponse<ProductReviewDto>.SuccessResponse(MapToDto(review), "Review submitted successfully and is pending moderation.");
    }

    public async Task<ApiResponse<bool>> VoteHelpfulAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _context.ProductReviews.FindAsync(new object[] { reviewId }, cancellationToken);
        if (review == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Review not found.");
        }

        review.HelpfulVotesCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Helpful vote recorded.");
    }

    public async Task<ApiResponse<PagedResult<ProductReviewDto>>> GetReviewsForModerationAsync(ReviewStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.ProductReviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var reviews = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = reviews.Select(r => MapToDto(r)).ToList();
        var pagedResult = new PagedResult<ProductReviewDto>(dtos, total, page, pageSize);

        return ApiResponse<PagedResult<ProductReviewDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<ProductReviewDto>> ModerateReviewAsync(Guid reviewId, ModerateReviewDto request, CancellationToken cancellationToken = default)
    {
        var review = await _context.ProductReviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
        {
            return ApiResponse<ProductReviewDto>.FailureResponse("Not Found", "Review not found.");
        }

        review.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate product aggregate ratings if approved or rejected
        await RecalculateProductRatingAsync(review.ProductId, cancellationToken);

        _logger.LogInformation("Review {ReviewId} status updated to {Status}", reviewId, request.Status);

        return ApiResponse<ProductReviewDto>.SuccessResponse(MapToDto(review), $"Review status updated to {request.Status}.");
    }

    public async Task<ApiResponse<ProductReviewDto>> AddAdminResponseAsync(Guid reviewId, AdminReviewResponseDto request, CancellationToken cancellationToken = default)
    {
        var review = await _context.ProductReviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
        {
            return ApiResponse<ProductReviewDto>.FailureResponse("Not Found", "Review not found.");
        }

        review.AdminResponse = request.Response.Trim();
        review.AdminRespondedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ProductReviewDto>.SuccessResponse(MapToDto(review), "Staff response recorded.");
    }

    public async Task<ApiResponse<bool>> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _context.ProductReviews.FindAsync(new object[] { reviewId }, cancellationToken);
        if (review == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Review not found.");
        }

        var productId = review.ProductId;
        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateProductRatingAsync(productId, cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Review deleted successfully.");
    }

    private async Task RecalculateProductRatingAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { productId }, cancellationToken);
        if (product == null) return;

        var approvedReviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .ToListAsync(cancellationToken);

        product.TotalReviews = approvedReviews.Count;
        product.AverageRating = approvedReviews.Any()
            ? Math.Round((decimal)approvedReviews.Average(r => r.Rating), 2)
            : 0m;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static ProductReviewDto MapToDto(ProductReview r)
    {
        return new ProductReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name ?? string.Empty,
            UserId = r.UserId,
            CustomerName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}".Trim() : "Anonymous",
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            Status = r.Status,
            HelpfulVotesCount = r.HelpfulVotesCount,
            AdminResponse = r.AdminResponse,
            AdminRespondedAtUtc = r.AdminRespondedAtUtc,
            CreatedAtUtc = r.CreatedAtUtc
        };
    }
}
