using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class CustomerProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string? PreferredCurrency { get; set; } = "USD";
    public string? PreferredLanguage { get; set; } = "en";
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal TotalSpent { get; set; } = 0m;
    public int OrdersCount { get; set; } = 0;
}
