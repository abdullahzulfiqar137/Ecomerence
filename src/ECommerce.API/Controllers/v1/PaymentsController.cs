using Asp.Versioning;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly IValidator<CreatePaymentIntentRequestDto> _intentValidator;
    private readonly IValidator<RefundRequestDto> _refundValidator;

    public PaymentsController(
        IPaymentService paymentService,
        IValidator<CreatePaymentIntentRequestDto> intentValidator,
        IValidator<RefundRequestDto> refundValidator)
    {
        _paymentService = paymentService;
        _intentValidator = intentValidator;
        _refundValidator = refundValidator;
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentsByOrderIdAsync(orderId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("intent")]
    [ProducesResponseType(typeof(ApiResponse<PaymentIntentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentIntentResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _intentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PaymentIntentResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _paymentService.CreatePaymentIntentAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
    [HttpPost("refund")]
    [ProducesResponseType(typeof(ApiResponse<RefundResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RefundResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRefund([FromBody] RefundRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _refundValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<RefundResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _paymentService.ProcessRefundAsync(request, CurrentUserEmail, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("webhook/{provider}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleWebhook(string provider, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString()
            ?? Request.Headers["X-Signature"].ToString()
            ?? "mock_sig";

        var result = await _paymentService.ProcessWebhookAsync(provider, payload, signature, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
