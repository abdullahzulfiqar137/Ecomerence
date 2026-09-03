using Asp.Versioning;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : BaseApiController
{
    private readonly ICatalogService _catalogService;
    private readonly IValidator<CreateProductDto> _createProductValidator;
    private readonly IValidator<UpdateProductDto> _updateProductValidator;

    public ProductsController(
        ICatalogService catalogService,
        IValidator<CreateProductDto> createProductValidator,
        IValidator<UpdateProductDto> updateProductValidator)
    {
        _catalogService = catalogService;
        _createProductValidator = createProductValidator;
        _updateProductValidator = updateProductValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _catalogService.GetProductsAsync(filterParams, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _catalogService.GetProductByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _catalogService.GetProductBySlugAsync(slug, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _createProductValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductDetailDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _catalogService.CreateProductAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _updateProductValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductDetailDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _catalogService.UpdateProductAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _catalogService.DeleteProductAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPost("{id:guid}/images")]
    [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductImage(Guid id, [FromBody] CreateProductImageDto request, CancellationToken cancellationToken)
    {
        var result = await _catalogService.AddProductImageAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await _catalogService.DeleteProductImageAsync(id, imageId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
