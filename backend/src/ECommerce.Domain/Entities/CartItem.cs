using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } // Price captured at the time item was added
    public bool IsSavedForLater { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
