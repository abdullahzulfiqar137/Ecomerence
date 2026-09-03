using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Catalog;

public class ProductAttributeService : IProductAttributeService
{
    private readonly IGenericRepository<ProductAttribute> _attributeRepo;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductAttributeService> _logger;

    public ProductAttributeService(
        IGenericRepository<ProductAttribute> attributeRepo,
        ApplicationDbContext context,
        ILogger<ProductAttributeService> logger)
    {
        _attributeRepo = attributeRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<ProductAttributeDto>>> GetAllAttributesAsync(CancellationToken cancellationToken = default)
    {
        var attributes = await _context.ProductAttributes
            .Include(a => a.Values)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var dtos = attributes.Select(a => new ProductAttributeDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            CommonValues = a.Values.Select(v => v.Value).Distinct().Take(20).ToList()
        }).ToList();

        return ApiResponse<IReadOnlyList<ProductAttributeDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<ProductAttributeDto>> GetAttributeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attribute = await _context.ProductAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attribute == null)
        {
            return ApiResponse<ProductAttributeDto>.FailureResponse("Not Found", "Attribute not found.");
        }

        var dto = new ProductAttributeDto
        {
            Id = attribute.Id,
            Name = attribute.Name,
            Description = attribute.Description,
            CommonValues = attribute.Values.Select(v => v.Value).Distinct().ToList()
        };

        return ApiResponse<ProductAttributeDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<ProductAttributeDto>> CreateAttributeAsync(CreateProductAttributeDto request, CancellationToken cancellationToken = default)
    {
        if (await _context.ProductAttributes.AnyAsync(a => a.Name == request.Name.Trim(), cancellationToken))
        {
            return ApiResponse<ProductAttributeDto>.FailureResponse("Conflict", $"Attribute with name '{request.Name}' already exists.");
        }

        var attribute = new ProductAttribute
        {
            Name = request.Name.Trim(),
            Description = request.Description
        };

        await _attributeRepo.AddAsync(attribute, cancellationToken);
        await _attributeRepo.SaveChangesAsync(cancellationToken);

        return await GetAttributeByIdAsync(attribute.Id, cancellationToken);
    }

    public async Task<ApiResponse<ProductAttributeDto>> UpdateAttributeAsync(Guid id, UpdateProductAttributeDto request, CancellationToken cancellationToken = default)
    {
        var attribute = await _attributeRepo.GetByIdAsync(id, cancellationToken);
        if (attribute == null)
        {
            return ApiResponse<ProductAttributeDto>.FailureResponse("Not Found", "Attribute not found.");
        }

        attribute.Name = request.Name.Trim();
        attribute.Description = request.Description;

        _attributeRepo.Update(attribute);
        await _attributeRepo.SaveChangesAsync(cancellationToken);

        return await GetAttributeByIdAsync(attribute.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAttributeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attribute = await _attributeRepo.GetByIdAsync(id, cancellationToken);
        if (attribute == null)
        {
            return ApiResponse<bool>.FailureResponse("Not Found", "Attribute not found.");
        }

        _attributeRepo.Delete(attribute);
        await _attributeRepo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Attribute deleted successfully.");
    }
}
