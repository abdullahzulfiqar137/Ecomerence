using Asp.Versioning;
using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/coupons")]
public class CouponsController : BaseApiController
{
    private readonly ICouponService _couponService;
    private readonly IValidator<CreateCouponDto> _createValidator;
    private readonly IValidator<UpdateCouponDto> _updateValidator;
    private readonly IValidator<ValidateCouponRequestDto> _validateValidator;

    public CouponsController(
        ICouponService couponService,
        IValidator<CreateCouponDto> createValidator,
        IValidator<UpdateCouponDto> updateValidator,
        IValidator<ValidateCouponRequestDto> validateValidator)
    {
        _couponService = couponService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _validateValidator = validateValidator;
    }

    [Authorize(Roles = "SuperAdmin,Admin,MarketingManager")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CouponDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isActiveOnly = null, CancellationToken cancellationToken = default)
    {
        var result = await _couponService.GetCouponsAsync(page, pageSize, isActiveOnly, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,MarketingManager")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetCouponByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ValidateCouponResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidateCouponResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _validateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ValidateCouponResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _couponService.ValidateCouponAsync(CurrentUserId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,MarketingManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CouponDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _couponService.CreateCouponAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,MarketingManager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CouponDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _couponService.UpdateCouponAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,MarketingManager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.DeleteCouponAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,MarketingManager")]
    [HttpGet("{id:guid}/usages")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CouponUsageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsages(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetCouponUsagesAsync(id, cancellationToken);
        return Ok(result);
    }
}
