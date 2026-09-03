using Asp.Versioning;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products/{productId:guid}/variants")]
public class ProductVariantsController : BaseApiController
{
    private readonly IProductVariantService _variantService;
    private readonly IValidator<CreateVariantDto> _createValidator;
    private readonly IValidator<UpdateVariantDto> _updateValidator;
    private readonly IValidator<GenerateVariantMatrixDto> _matrixValidator;

    public ProductVariantsController(
        IProductVariantService variantService,
        IValidator<CreateVariantDto> createValidator,
        IValidator<UpdateVariantDto> updateValidator,
        IValidator<GenerateVariantMatrixDto> matrixValidator)
    {
        _variantService = variantService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _matrixValidator = matrixValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VariantDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVariants(Guid productId, CancellationToken cancellationToken = default)
    {
        var result = await _variantService.GetVariantsByProductIdAsync(productId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{variantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariantById(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
        var result = await _variantService.GetVariantByIdAsync(variantId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVariant(Guid productId, [FromBody] CreateVariantDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VariantDetailDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _variantService.CreateVariantAsync(productId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetVariantById), new { productId, variantId = result.Data!.Id }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPost("matrix")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VariantDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VariantDetailDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateMatrix(Guid productId, [FromBody] GenerateVariantMatrixDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _matrixValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<IReadOnlyList<VariantDetailDto>>.FailureResponse("Validation failed", errors));
        }

        var result = await _variantService.GenerateMatrixAsync(productId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPut("{variantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VariantDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid variantId, [FromBody] UpdateVariantDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VariantDetailDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _variantService.UpdateVariantAsync(productId, variantId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpDelete("{variantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVariant(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
        var result = await _variantService.DeleteVariantAsync(productId, variantId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
