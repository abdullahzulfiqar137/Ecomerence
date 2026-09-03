using Asp.Versioning;
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/attributes")]
public class ProductAttributesController : BaseApiController
{
    private readonly IProductAttributeService _attributeService;
    private readonly IValidator<CreateProductAttributeDto> _createValidator;
    private readonly IValidator<UpdateProductAttributeDto> _updateValidator;

    public ProductAttributesController(
        IProductAttributeService attributeService,
        IValidator<CreateProductAttributeDto> createValidator,
        IValidator<UpdateProductAttributeDto> updateValidator)
    {
        _attributeService = attributeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProductAttributeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.GetAllAttributesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.GetAttributeByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductAttributeDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductAttributeDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _attributeService.CreateAttributeAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductAttributeDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductAttributeDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _attributeService.UpdateAttributeAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.DeleteAttributeAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
