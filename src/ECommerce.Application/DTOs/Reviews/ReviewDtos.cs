using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Reviews;

public class ProductReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public ReviewStatus Status { get; set; }
    public int HelpfulVotesCount { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime? AdminRespondedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateProductReviewDto
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; } // 1 to 5
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

public class UpdateProductReviewDto
{
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

public class ModerateReviewDto
{
    public ReviewStatus Status { get; set; } // Approved or Rejected
}

public class AdminReviewResponseDto
{
    public string Response { get; set; } = string.Empty;
}

public class ProductReviewSummaryDto
{
    public Guid ProductId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
}
