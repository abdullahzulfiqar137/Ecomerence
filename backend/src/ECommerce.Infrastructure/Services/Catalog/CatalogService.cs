using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Application.Specifications;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Catalog;

public class CatalogService : ICatalogService
{
    private readonly IGenericRepository<Product> _productRepo;
    private readonly IGenericRepository<ProductImage> _imageRepo;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(
        IGenericRepository<Product> productRepo,
        IGenericRepository<ProductImage> imageRepo,
        ApplicationDbContext context,
        ILogger<CatalogService> logger)
    {
        _productRepo = productRepo;
        _imageRepo = imageRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ProductListDto>>> GetProductsAsync(ProductFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var spec = new ProductWithFilterSpecification(filterParams);
        var countSpec = new ProductWithFilterForCountSpecification(filterParams);

        var totalItems = await _productRepo.CountAsync(countSpec, cancellationToken);
        var products = await _productRepo.ListWithSpecAsync(spec, cancellationToken);

        var dtos = products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            SKU = p.SKU,
            Price = p.Price,
            SalePrice = p.SalePrice,
            MainImageUrl = p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl
                          ?? p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
            CategoryName = p.Category?.Name ?? string.Empty,
            BrandName = p.Brand?.Name ?? string.Empty,
            IsInStock = true,
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            IsFeatured = p.IsFeatured,
            Type = p.Type
        }).ToList();

        var pagedResult = new PagedResult<ProductListDto>(dtos, totalItems, filterParams.Page, filterParams.PageSize);
        return ApiResponse<PagedResult<ProductListDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<ProductDetailDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = new ProductWithDetailsSpecification(id);
        var product = await _productRepo.GetEntityWithSpecAsync(spec, cancellationToken);

        if (product == null)
        {
            return ApiResponse<ProductDetailDto>.FailureResponse("Not Found", "Product not found.");
        }

        return ApiResponse<ProductDetailDto>.SuccessResponse(MapToDetailDto(product));
    }

    public async Task<ApiResponse<ProductDetailDto>> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var spec = new ProductWithDetailsSpecification(slug);
        var product = await _productRepo.GetEntityWithSpecAsync(spec, cancellationToken);

        if (product == null)
        {
            return ApiResponse<ProductDetailDto>.FailureResponse("Not Found", $"Product with slug '{slug}' not found.");
        }

        return ApiResponse<ProductDetailDto>.SuccessResponse(MapToDetailDto(product));
    }

    public async Task<ApiResponse<ProductDetailDto>> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : GenerateSlug(request.Slug);

        // Check unique SKU
        if (await _context.Products.AnyAsync(p => p.SKU == request.SKU, cancellationToken))
        {
            return ApiResponse<ProductDetailDto>.FailureResponse("Conflict", $"A product with SKU '{request.SKU}' already exists.");
        }

        // Check unique Slug
        if (await _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            SKU = request.SKU.Trim(),
            ShortDescription = request.ShortDescription,
            FullDescription = request.FullDescription,
            Price = request.Price,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            Type = request.Type,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            TrackInventory = request.TrackInventory,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription
        };

        if (request.Images.Any())
        {
            foreach (var img in request.Images)
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = img.ImageUrl,
                    AltText = img.AltText,
                    IsMain = img.IsMain,
                    DisplayOrder = img.DisplayOrder,
                    ProductVariantId = img.ProductVariantId
                });
            }
        }

        await _productRepo.AddAsync(product, cancellationToken);
        await _productRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created product {ProductName} with ID {ProductId}", product.Name, product.Id);

        return await GetProductByIdAsync(product.Id, cancellationToken);
    }

    public async Task<ApiResponse<ProductDetailDto>> UpdateProductAsync(Guid id, UpdateProductDto request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
        {
            return ApiResponse<ProductDetailDto>.FailureResponse("Not Found", "Product not found.");
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : GenerateSlug(request.Slug);

        product.Name = request.Name.Trim();
        product.Slug = slug;
        product.SKU = request.SKU.Trim();
        product.ShortDescription = request.ShortDescription;
        product.FullDescription = request.FullDescription;
        product.Price = request.Price;
        product.SalePrice = request.SalePrice;
        product.CostPrice = request.CostPrice;
        product.Type = request.Type;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.TrackInventory = request.TrackInventory;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.MetaTitle = request.MetaTitle;
        product.MetaDescription = request.MetaDescription;

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync(cancellationToken);

        return await GetProductByIdAsync(product.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepo.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Product not found.");
        }

        _productRepo.Delete(product);
        await _productRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully.");
    }

    public async Task<ApiResponse<ProductImageDto>> AddProductImageAsync(Guid productId, CreateProductImageDto request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return ApiResponse<ProductImageDto>.FailureResponse("Not Found", "Product not found.");
        }

        var image = new ProductImage
        {
            ProductId = productId,
            ProductVariantId = request.ProductVariantId,
            ImageUrl = request.ImageUrl,
            AltText = request.AltText,
            IsMain = request.IsMain,
            DisplayOrder = request.DisplayOrder
        };

        await _imageRepo.AddAsync(image, cancellationToken);
        await _imageRepo.SaveChangesAsync(cancellationToken);

        var dto = new ProductImageDto
        {
            Id = image.Id,
            ProductId = image.ProductId,
            ProductVariantId = image.ProductVariantId,
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            IsMain = image.IsMain,
            DisplayOrder = image.DisplayOrder
        };

        return ApiResponse<ProductImageDto>.SuccessResponse(dto, "Image added successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteProductImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _imageRepo.GetByIdAsync(imageId, cancellationToken);
        if (image == null || image.ProductId != productId)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Image not found.");
        }

        _imageRepo.Delete(image);
        await _imageRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Image deleted successfully.");
    }

    private static ProductDetailDto MapToDetailDto(Product p)
    {
        return new ProductDetailDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            SKU = p.SKU,
            ShortDescription = p.ShortDescription,
            FullDescription = p.FullDescription,
            Price = p.Price,
            SalePrice = p.SalePrice,
            CostPrice = p.CostPrice,
            Currency = p.Currency,
            Type = p.Type,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            TrackInventory = p.TrackInventory,
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            BrandId = p.BrandId,
            BrandName = p.Brand?.Name ?? string.Empty,
            MetaTitle = p.MetaTitle,
            MetaDescription = p.MetaDescription,
            Images = p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductVariantId = i.ProductVariantId,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsMain = i.IsMain,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            Variants = p.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                SKU = v.SKU,
                Name = v.Name,
                Price = v.Price,
                SalePrice = v.SalePrice,
                IsActive = v.IsActive,
                AvailableStock = 0
            }).ToList()
        };
    }

    private static string GenerateSlug(string text)
    {
        var str = text.ToLowerInvariant().Trim();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-").Trim('-');
        return str;
    }
}
