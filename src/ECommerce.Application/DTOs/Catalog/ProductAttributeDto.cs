namespace ECommerce.Application.DTOs.Catalog;

public class ProductAttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> CommonValues { get; set; } = new();
}

public class CreateProductAttributeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProductAttributeDto : CreateProductAttributeDto
{
}

public class ProductAttributeValueDto
{
    public Guid Id { get; set; }
    public Guid ProductAttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public Guid ProductVariantId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class AssignAttributeValueDto
{
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
