using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class ProductReview : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public int Rating { get; set; } // 1 to 5
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; } = false;
    public ReviewStatus Status { get; set; } = ReviewStatus.PendingModeration;
    public int HelpfulVotesCount { get; set; } = 0;

    public string? AdminResponse { get; set; }
    public DateTime? AdminRespondedAtUtc { get; set; }
}
