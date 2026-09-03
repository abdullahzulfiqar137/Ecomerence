using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Catalog;

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string? MainImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public bool IsInStock { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsFeatured { get; set; }
    public ProductType Type { get; set; }
}

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public ProductType Type { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool TrackInventory { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsActive { get; set; }
    public int AvailableStock { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public ProductType Type { get; set; } = ProductType.Simple;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool TrackInventory { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public List<CreateProductImageDto> Images { get; set; } = new();
}

public class UpdateProductDto : CreateProductDto
{
}
