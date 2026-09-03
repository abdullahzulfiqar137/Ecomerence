namespace ECommerce.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    string? DeletedBy { get; set; }
}
