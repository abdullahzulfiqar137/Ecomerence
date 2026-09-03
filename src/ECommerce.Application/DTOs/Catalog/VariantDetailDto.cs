namespace ECommerce.Application.DTOs.Catalog;

public class VariantDetailDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? ColorCode { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int AvailableStock { get; set; }
    public List<ProductAttributeValueDto> Attributes { get; set; } = new();
}

public class CreateVariantDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? ColorCode { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public List<AssignAttributeValueDto> Attributes { get; set; } = new();
}

public class UpdateVariantDto : CreateVariantDto
{
}

public class GenerateVariantMatrixDto
{
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public List<AttributeOptionGroupDto> OptionGroups { get; set; } = new();
}

public class AttributeOptionGroupDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}
