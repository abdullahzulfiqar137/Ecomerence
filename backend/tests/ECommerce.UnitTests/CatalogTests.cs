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

public class CatalogTests
{
    private readonly ApplicationDbContext _context;
    private readonly CatalogService _catalogService;
    private readonly CategoryService _categoryService;
    private readonly BrandService _brandService;

    public CatalogTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        var productRepo = new GenericRepository<Product>(_context);
        var imageRepo = new GenericRepository<ProductImage>(_context);
        var categoryRepo = new GenericRepository<Category>(_context);
        var brandRepo = new GenericRepository<Brand>(_context);

        _catalogService = new CatalogService(productRepo, imageRepo, _context, NullLogger<CatalogService>.Instance);
        _categoryService = new CategoryService(categoryRepo, _context, NullLogger<CategoryService>.Instance);
        _brandService = new BrandService(brandRepo, _context, NullLogger<BrandService>.Instance);
    }

    [Fact]
    public async Task GetProductsAsync_WithPagingAndFiltering_ShouldReturnPagedResults()
    {
        var filterParams = new ProductFilterParams
        {
            Page = 1,
            PageSize = 10
        };

        var response = await _catalogService.GetProductsAsync(filterParams);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCountGreaterThan(0);
        response.Data.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetProductBySlugAsync_ValidSlug_ShouldReturnProductDetails()
    {
        var response = await _catalogService.GetProductBySlugAsync("titanium-pro-smartphone-256gb");

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be("Titanium Pro Smartphone 256GB");
        response.Data.CategoryName.Should().Be("Electronics");
        response.Data.BrandName.Should().Be("Apple");
    }

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ShouldPersistSuccessfully()
    {
        var category = await _context.Categories.FirstAsync();
        var brand = await _context.Brands.FirstAsync();

        var request = new CreateProductDto
        {
            Name = "Wireless Noise-Cancelling Headphones Pro",
            SKU = "WNC-HP-990",
            Price = 299.99m,
            CategoryId = category.Id,
            BrandId = brand.Id,
            ShortDescription = "Premium ANC headphones with 40-hour battery life."
        };

        var response = await _catalogService.CreateProductAsync(request);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.SKU.Should().Be("WNC-HP-990");

        var dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == "WNC-HP-990");
        dbProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategoryTreeAsync_ShouldReturnHierarchicalStructure()
    {
        var response = await _categoryService.GetCategoryTreeAsync();

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAllBrandsAsync_ShouldReturnAllActiveBrands()
    {
        var response = await _brandService.GetAllBrandsAsync();

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCount(6);
    }
}
