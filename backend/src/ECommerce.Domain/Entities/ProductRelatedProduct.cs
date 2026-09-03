namespace ECommerce.Domain.Entities;

public class ProductRelatedProduct
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid RelatedProductId { get; set; }
    public Product? RelatedProduct { get; set; }
}
