using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Catalog;

public class ProductVariantService : IProductVariantService
{
    private readonly IGenericRepository<ProductVariant> _variantRepo;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductVariantService> _logger;

    public ProductVariantService(
        IGenericRepository<ProductVariant> variantRepo,
        ApplicationDbContext context,
        ILogger<ProductVariantService> logger)
    {
        _variantRepo = variantRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<VariantDetailDto>>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var variants = await _context.ProductVariants
            .Include(v => v.Product)
            .Include(v => v.AttributeValues)
                .ThenInclude(av => av.ProductAttribute)
            .Include(v => v.InventoryItems)
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(cancellationToken);

        var dtos = variants.Select(v => MapToDto(v)).ToList();
        return ApiResponse<IReadOnlyList<VariantDetailDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<VariantDetailDto>> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .Include(v => v.AttributeValues)
                .ThenInclude(av => av.ProductAttribute)
            .Include(v => v.InventoryItems)
            .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);

        if (variant == null)
        {
            return ApiResponse<VariantDetailDto>.FailureResponse("Not Found", "Variant not found.");
        }

        return ApiResponse<VariantDetailDto>.SuccessResponse(MapToDto(variant));
    }

    public async Task<ApiResponse<VariantDetailDto>> CreateVariantAsync(Guid productId, CreateVariantDto request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return ApiResponse<VariantDetailDto>.FailureResponse("Not Found", "Product not found.");
        }

        if (await _context.ProductVariants.AnyAsync(v => v.SKU == request.SKU.Trim(), cancellationToken))
        {
            return ApiResponse<VariantDetailDto>.FailureResponse("Conflict", $"A variant with SKU '{request.SKU}' already exists.");
        }

        var variant = new ProductVariant
        {
            ProductId = productId,
            Name = request.Name.Trim(),
            SKU = request.SKU.Trim(),
            Barcode = request.Barcode?.Trim(),
            Price = request.Price,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            Size = request.Size,
            Color = request.Color,
            ColorCode = request.ColorCode,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        if (request.Attributes.Any())
        {
            foreach (var attr in request.Attributes)
            {
                variant.AttributeValues.Add(new ProductAttributeValue
                {
                    ProductAttributeId = attr.ProductAttributeId,
                    Value = attr.Value.Trim()
                });
            }
        }

        await _variantRepo.AddAsync(variant, cancellationToken);
        await _variantRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created variant {VariantSKU} for product {ProductId}", variant.SKU, productId);

        return await GetVariantByIdAsync(variant.Id, cancellationToken);
    }

    public async Task<ApiResponse<VariantDetailDto>> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantDto request, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.AttributeValues)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId, cancellationToken);

        if (variant == null)
        {
            return ApiResponse<VariantDetailDto>.FailureResponse("Not Found", "Variant not found.");
        }

        variant.Name = request.Name.Trim();
        variant.SKU = request.SKU.Trim();
        variant.Barcode = request.Barcode?.Trim();
        variant.Price = request.Price;
        variant.SalePrice = request.SalePrice;
        variant.CostPrice = request.CostPrice;
        variant.Size = request.Size;
        variant.Color = request.Color;
        variant.ColorCode = request.ColorCode;
        variant.Weight = request.Weight;
        variant.Length = request.Length;
        variant.Width = request.Width;
        variant.Height = request.Height;
        variant.ImageUrl = request.ImageUrl;
        variant.IsActive = request.IsActive;
        variant.DisplayOrder = request.DisplayOrder;

        // Update attribute values
        variant.AttributeValues.Clear();
        if (request.Attributes.Any())
        {
            foreach (var attr in request.Attributes)
            {
                variant.AttributeValues.Add(new ProductAttributeValue
                {
                    ProductAttributeId = attr.ProductAttributeId,
                    Value = attr.Value.Trim(),
                    ProductVariantId = variant.Id
                });
            }
        }

        _variantRepo.Update(variant);
        await _variantRepo.SaveChangesAsync(cancellationToken);

        return await GetVariantByIdAsync(variant.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId, cancellationToken);
        if (variant == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Variant not found.");
        }

        _variantRepo.Delete(variant);
        await _variantRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Variant deleted successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<VariantDetailDto>>> GenerateMatrixAsync(Guid productId, GenerateVariantMatrixDto request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return ApiResponse<IReadOnlyList<VariantDetailDto>>.FailureResponse("Not Found", "Product not found.");
        }

        // Cartesian product of option groups
        var combinations = GenerateCartesianProduct(request.OptionGroups);
        var createdVariants = new List<ProductVariant>();

        var displayOrder = 1;
        foreach (var combo in combinations)
        {
            var variantName = string.Join(" / ", combo.Select(c => c.Value));
            var skuSuffix = string.Join("-", combo.Select(c => SanitizeSkuPart(c.Value)));
            var sku = $"{product.SKU}-{skuSuffix}".ToUpperInvariant();

            // Ensure unique SKU
            var uniqueSku = sku;
            var counter = 1;
            while (await _context.ProductVariants.AnyAsync(v => v.SKU == uniqueSku, cancellationToken) ||
                   createdVariants.Any(v => v.SKU == uniqueSku))
            {
                uniqueSku = $"{sku}-{counter++}";
            }

            var variant = new ProductVariant
            {
                ProductId = productId,
                Name = variantName,
                SKU = uniqueSku,
                Price = request.BasePrice,
                SalePrice = request.SalePrice,
                IsActive = true,
                DisplayOrder = displayOrder++,
                Size = combo.FirstOrDefault(c => c.AttributeName.Equals("Size", StringComparison.OrdinalIgnoreCase))?.Value,
                Color = combo.FirstOrDefault(c => c.AttributeName.Equals("Color", StringComparison.OrdinalIgnoreCase))?.Value
            };

            foreach (var opt in combo)
            {
                variant.AttributeValues.Add(new ProductAttributeValue
                {
                    ProductAttributeId = opt.AttributeId,
                    Value = opt.Value
                });
            }

            createdVariants.Add(variant);
        }

        await _context.ProductVariants.AddRangeAsync(createdVariants, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated {VariantCount} variants for product {ProductName}", createdVariants.Count, product.Name);

        return await GetVariantsByProductIdAsync(productId, cancellationToken);
    }

    private static VariantDetailDto MapToDto(ProductVariant v)
    {
        var availableStock = v.InventoryItems.Sum(i => i.QuantityAvailable);

        return new VariantDetailDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            ProductName = v.Product?.Name ?? string.Empty,
            Name = v.Name,
            SKU = v.SKU,
            Barcode = v.Barcode,
            Price = v.Price,
            SalePrice = v.SalePrice,
            CostPrice = v.CostPrice,
            Size = v.Size,
            Color = v.Color,
            ColorCode = v.ColorCode,
            Weight = v.Weight,
            Length = v.Length,
            Width = v.Width,
            Height = v.Height,
            ImageUrl = v.ImageUrl,
            IsActive = v.IsActive,
            DisplayOrder = v.DisplayOrder,
            AvailableStock = availableStock,
            Attributes = v.AttributeValues.Select(av => new ProductAttributeValueDto
            {
                Id = av.Id,
                ProductAttributeId = av.ProductAttributeId,
                AttributeName = av.ProductAttribute?.Name ?? string.Empty,
                ProductVariantId = av.ProductVariantId,
                Value = av.Value
            }).ToList()
        };
    }

    private class OptionItem
    {
        public Guid AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private static List<List<OptionItem>> GenerateCartesianProduct(List<AttributeOptionGroupDto> groups)
    {
        var result = new List<List<OptionItem>> { new() };

        foreach (var group in groups)
        {
            var next = new List<List<OptionItem>>();
            foreach (var current in result)
            {
                foreach (var opt in group.Options)
                {
                    var copy = new List<OptionItem>(current)
                    {
                        new() { AttributeId = group.AttributeId, AttributeName = group.AttributeName, Value = opt }
                    };
                    next.Add(copy);
                }
            }
            result = next;
        }

        return result;
    }

    private static string SanitizeSkuPart(string text)
    {
        var str = text.Trim().ToUpperInvariant();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[^A-Z0-9]", "");
        return str.Length > 4 ? str[..4] : str;
    }
}
