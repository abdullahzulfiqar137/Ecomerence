using Asp.Versioning;
using ECommerce.API.Middlewares;
using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checkout")]
[EnableRateLimiting("CheckoutPolicy")]
public class CheckoutController : BaseApiController
{
    private readonly ICheckoutService _checkoutService;
    private readonly IValidator<CheckoutQuoteRequestDto> _quoteValidator;
    private readonly IValidator<CheckoutRequestDto> _checkoutValidator;

    public CheckoutController(
        ICheckoutService checkoutService,
        IValidator<CheckoutQuoteRequestDto> quoteValidator,
        IValidator<CheckoutRequestDto> checkoutValidator)
    {
        _checkoutService = checkoutService;
        _quoteValidator = quoteValidator;
        _checkoutValidator = checkoutValidator;
    }

    [HttpPost("quote")]
    [ProducesResponseType(typeof(ApiResponse<CheckoutQuoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CheckoutQuoteResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetQuote([FromBody] CheckoutQuoteRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _quoteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CheckoutQuoteResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _checkoutService.CalculateQuoteAsync(CurrentUserId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("process")]
    [RequireIdempotency(Required = false)]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _checkoutValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CheckoutResultDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _checkoutService.ProcessCheckoutAsync(CurrentUserId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
