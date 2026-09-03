using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Catalog;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepo;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IGenericRepository<Category> categoryRepo,
        ApplicationDbContext context,
        ILogger<CategoryService> logger)
    {
        _categoryRepo = categoryRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var dtos = categories.Select(c => MapToDto(c)).ToList();
        return ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
    {
        var allCategories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var rootCategories = allCategories
            .Where(c => c.ParentCategoryId == null)
            .Select(c => BuildTree(c, allCategories))
            .ToList();

        return ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(rootCategories);
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            return ApiResponse<CategoryDto>.FailureResponse("Not Found", "Category not found.");
        }

        return ApiResponse<CategoryDto>.SuccessResponse(MapToDto(category));
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : GenerateSlug(request.Slug);

        if (await _context.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured
        };

        await _categoryRepo.AddAsync(category, cancellationToken);
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        return await GetCategoryByIdAsync(category.Id, cancellationToken);
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepo.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return ApiResponse<CategoryDto>.FailureResponse("Not Found", "Category not found.");
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : GenerateSlug(request.Slug);

        category.Name = request.Name.Trim();
        category.Slug = slug;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.ParentCategoryId = request.ParentCategoryId;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.IsFeatured = request.IsFeatured;

        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        return await GetCategoryByIdAsync(category.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepo.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Category not found.");
        }

        _categoryRepo.Delete(category);
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully.");
    }

    private static CategoryDto MapToDto(Category c)
    {
        return new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            IsFeatured = c.IsFeatured,
            Children = c.SubCategories.Select(sc => MapToDto(sc)).ToList()
        };
    }

    private static CategoryDto BuildTree(Category parent, List<Category> all)
    {
        var dto = MapToDto(parent);
        dto.Children = all
            .Where(c => c.ParentCategoryId == parent.Id)
            .Select(c => BuildTree(c, all))
            .ToList();
        return dto;
    }

    private static string GenerateSlug(string text)
    {
        var str = text.ToLowerInvariant().Trim();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-").Trim('-');
        return str;
    }
}
