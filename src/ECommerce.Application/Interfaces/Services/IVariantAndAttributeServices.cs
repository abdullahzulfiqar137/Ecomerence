using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface IProductAttributeService
{
    Task<ApiResponse<IReadOnlyList<ProductAttributeDto>>> GetAllAttributesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> GetAttributeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> CreateAttributeAsync(CreateProductAttributeDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> UpdateAttributeAsync(Guid id, UpdateProductAttributeDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAttributeAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IProductVariantService
{
    Task<ApiResponse<IReadOnlyList<VariantDetailDto>>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ApiResponse<VariantDetailDto>> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<ApiResponse<VariantDetailDto>> CreateVariantAsync(Guid productId, CreateVariantDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<VariantDetailDto>> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<VariantDetailDto>>> GenerateMatrixAsync(Guid productId, GenerateVariantMatrixDto request, CancellationToken cancellationToken = default);
}
