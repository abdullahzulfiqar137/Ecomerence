using Asp.Versioning;
using ECommerce.Application.DTOs.International;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/currencies")]
public class CurrenciesController : BaseApiController
{
    private readonly ICurrencyService _currencyService;
    private readonly IValidator<CreateCurrencyDto> _createValidator;
    private readonly IValidator<UpdateCurrencyRateDto> _updateRateValidator;
    private readonly IValidator<ConvertCurrencyRequestDto> _convertValidator;

    public CurrenciesController(
        ICurrencyService currencyService,
        IValidator<CreateCurrencyDto> createValidator,
        IValidator<UpdateCurrencyRateDto> updateRateValidator,
        IValidator<ConvertCurrencyRequestDto> convertValidator)
    {
        _currencyService = currencyService;
        _createValidator = createValidator;
        _updateRateValidator = updateRateValidator;
        _convertValidator = convertValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CurrencyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var result = await _currencyService.GetAllCurrenciesAsync(isActiveOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _currencyService.GetCurrencyByCodeAsync(code, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("convert")]
    [ProducesResponseType(typeof(ApiResponse<ConvertCurrencyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ConvertCurrencyResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConvertCurrency([FromBody] ConvertCurrencyRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _convertValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ConvertCurrencyResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _currencyService.ConvertCurrencyAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CurrencyDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _currencyService.CreateCurrencyAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetByCode), new { code = result.Data!.Code }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
    [HttpPatch("{code}/rate")]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRate(string code, [FromBody] UpdateCurrencyRateDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _updateRateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CurrencyDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _currencyService.UpdateExchangeRateAsync(code, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("countries")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CountryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries([FromQuery] bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var result = await _currencyService.GetAllCountriesAsync(isActiveOnly, cancellationToken);
        return Ok(result);
    }
}
