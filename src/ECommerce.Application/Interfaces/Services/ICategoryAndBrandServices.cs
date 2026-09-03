using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IBrandService
{
    Task<ApiResponse<IReadOnlyList<BrandDto>>> GetAllBrandsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<BrandDto>> GetBrandByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<BrandDto>> CreateBrandAsync(CreateBrandDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<BrandDto>> UpdateBrandAsync(Guid id, UpdateBrandDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default);
}
