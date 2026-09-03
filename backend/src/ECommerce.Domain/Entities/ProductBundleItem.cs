using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductBundleItem : BaseEntity
{
    public Guid BundleProductId { get; set; }
    public Product? BundleProduct { get; set; }

    public Guid ItemProductId { get; set; }
    public Product? ItemProduct { get; set; }

    public int Quantity { get; set; } = 1;
    public decimal? DiscountPercentage { get; set; }
}
