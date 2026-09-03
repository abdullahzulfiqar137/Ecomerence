using Asp.Versioning;
using ECommerce.Application.DTOs.International;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tax-rules")]
public class TaxRulesController : BaseApiController
{
    private readonly ITaxService _taxService;
    private readonly IValidator<CreateTaxRuleDto> _createValidator;
    private readonly IValidator<UpdateTaxRuleDto> _updateValidator;
    private readonly IValidator<CalculateTaxRequestDto> _calculateValidator;

    public TaxRulesController(
        ITaxService taxService,
        IValidator<CreateTaxRuleDto> createValidator,
        IValidator<UpdateTaxRuleDto> updateValidator,
        IValidator<CalculateTaxRequestDto> calculateValidator)
    {
        _taxService = taxService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _calculateValidator = calculateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TaxRuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var result = await _taxService.GetAllTaxRulesAsync(isActiveOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taxService.GetTaxRuleByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("calculate")]
    [ProducesResponseType(typeof(ApiResponse<CalculateTaxResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CalculateTaxResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateTax([FromBody] CalculateTaxRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _calculateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CalculateTaxResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _taxService.CalculateTaxAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaxRuleDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<TaxRuleDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _taxService.CreateTaxRuleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRuleDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<TaxRuleDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _taxService.UpdateTaxRuleAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taxService.DeleteTaxRuleAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
