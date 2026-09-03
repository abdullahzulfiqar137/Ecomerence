using ECommerce.Application.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Catalog;

public class ProductFilterParams : PaginationParams
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? InStockOnly { get; set; }
    public ProductType? Type { get; set; }
}
