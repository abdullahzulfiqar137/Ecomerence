using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class OrderNote : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string Note { get; set; } = string.Empty;
    public bool IsCustomerVisible { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
