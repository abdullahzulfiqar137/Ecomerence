using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Catalog;

public class BrandService : IBrandService
{
    private readonly IGenericRepository<Brand> _brandRepo;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BrandService> _logger;

    public BrandService(
        IGenericRepository<Brand> brandRepo,
        ApplicationDbContext context,
        ILogger<BrandService> logger)
    {
        _brandRepo = brandRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<BrandDto>>> GetAllBrandsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Brands
            .Include(b => b.Products)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        var brands = await query
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        var dtos = brands.Select(b => MapToDto(b)).ToList();
        return ApiResponse<IReadOnlyList<BrandDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<BrandDto>> GetBrandByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (brand == null)
        {
            return ApiResponse<BrandDto>.FailureResponse("Not Found", "Brand not found.");
        }

        return ApiResponse<BrandDto>.SuccessResponse(MapToDto(brand));
    }

    public async Task<ApiResponse<BrandDto>> CreateBrandAsync(CreateBrandDto request, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : GenerateSlug(request.Slug);

        if (await _context.Brands.AnyAsync(b => b.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        var brand = new Brand
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            Website = request.Website,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured
        };

        await _brandRepo.AddAsync(brand, cancellationToken);
        await _brandRepo.SaveChangesAsync(cancellationToken);

        return await GetBrandByIdAsync(brand.Id, cancellationToken);
    }

    public async Task<ApiResponse<BrandDto>> UpdateBrandAsync(Guid id, UpdateBrandDto request, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepo.GetByIdAsync(id, cancellationToken);
        if (brand == null)
        {
            return ApiResponse<BrandDto>.FailureResponse("Not Found", "Brand not found.");
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : GenerateSlug(request.Slug);

        brand.Name = request.Name.Trim();
        brand.Slug = slug;
        brand.Description = request.Description;
        brand.LogoUrl = request.LogoUrl;
        brand.Website = request.Website;
        brand.IsActive = request.IsActive;
        brand.IsFeatured = request.IsFeatured;

        _brandRepo.Update(brand);
        await _brandRepo.SaveChangesAsync(cancellationToken);

        return await GetBrandByIdAsync(brand.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepo.GetByIdAsync(id, cancellationToken);
        if (brand == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Brand not found.");
        }

        _brandRepo.Delete(brand);
        await _brandRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Brand deleted successfully.");
    }

    private static BrandDto MapToDto(Brand b)
    {
        return new BrandDto
        {
            Id = b.Id,
            Name = b.Name,
            Slug = b.Slug,
            Description = b.Description,
            LogoUrl = b.LogoUrl,
            Website = b.Website,
            IsActive = b.IsActive,
            IsFeatured = b.IsFeatured,
            ProductCount = b.Products.Count
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
