using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ICatalogService
{
    Task<ApiResponse<PagedResult<ProductListDto>>> GetProductsAsync(ProductFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> UpdateProductAsync(Guid id, UpdateProductDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductImageDto>> AddProductImageAsync(Guid productId, CreateProductImageDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteProductImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);
}
