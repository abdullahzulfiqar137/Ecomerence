using ECommerce.Application.DTOs.Catalog;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services.Catalog;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class VariantTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProductVariantService _variantService;
    private readonly ProductAttributeService _attributeService;

    public VariantTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        var variantRepo = new GenericRepository<ProductVariant>(_context);
        var attributeRepo = new GenericRepository<ProductAttribute>(_context);

        _variantService = new ProductVariantService(variantRepo, _context, NullLogger<ProductVariantService>.Instance);
        _attributeService = new ProductAttributeService(attributeRepo, _context, NullLogger<ProductAttributeService>.Instance);
    }

    [Fact]
    public async Task CreateAttributeAsync_ValidAttribute_ShouldPersistSuccessfully()
    {
        var request = new CreateProductAttributeDto
        {
            Name = "Shoe Size",
            Description = "Standard US/EU shoe sizing"
        };

        var response = await _attributeService.CreateAttributeAsync(request);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be("Shoe Size");
    }

    [Fact]
    public async Task GetVariantsByProductIdAsync_ShouldReturnProductVariants()
    {
        var product = await _context.Products.FirstAsync(p => p.Variants.Any());

        var response = await _variantService.GetVariantsByProductIdAsync(product.Id);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCount(5); // Seeded 5 variants for T-Shirt
    }

    [Fact]
    public async Task CreateVariantAsync_ValidVariant_ShouldPersistAndLinkAttributes()
    {
        var product = await _context.Products.FirstAsync(p => p.Variants.Any());
        var attr = await _context.ProductAttributes.FirstOrDefaultAsync() ?? new ProductAttribute { Name = "Fabric" };
        if (attr.Id == Guid.Empty)
        {
            await _context.ProductAttributes.AddAsync(attr);
            await _context.SaveChangesAsync();
        }

        var request = new CreateVariantDto
        {
            Name = "Extra Large / Red",
            SKU = "TSHIRT-SIG-XL-RED",
            Price = 27.99m,
            SalePrice = 24.99m,
            Size = "XL",
            Color = "Red",
            Attributes = new List<AssignAttributeValueDto>
            {
                new() { ProductAttributeId = attr.Id, Value = "100% Organic Silk" }
            }
        };

        var response = await _variantService.CreateVariantAsync(product.Id, request);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.SKU.Should().Be("TSHIRT-SIG-XL-RED");
        response.Data.Attributes.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateMatrixAsync_WithMultipleOptionGroups_ShouldGenerateCombinations()
    {
        var product = await _context.Products.FirstAsync(p => !p.Variants.Any());

        var sizeAttr = new ProductAttribute { Name = "Size" };
        var colorAttr = new ProductAttribute { Name = "Color" };
        await _context.ProductAttributes.AddRangeAsync(sizeAttr, colorAttr);
        await _context.SaveChangesAsync();

        var matrixRequest = new GenerateVariantMatrixDto
        {
            BasePrice = 99.99m,
            SalePrice = 89.99m,
            OptionGroups = new List<AttributeOptionGroupDto>
            {
                new()
                {
                    AttributeId = sizeAttr.Id,
                    AttributeName = "Size",
                    Options = new List<string> { "Small", "Medium", "Large" }
                },
                new()
                {
                    AttributeId = colorAttr.Id,
                    AttributeName = "Color",
                    Options = new List<string> { "Red", "Blue" }
                }
            }
        };

        // 3 sizes x 2 colors = 6 generated variants
        var response = await _variantService.GenerateMatrixAsync(product.Id, matrixRequest);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCount(6);
        response.Data!.All(v => v.Price == 99.99m).Should().BeTrue();
    }
}
