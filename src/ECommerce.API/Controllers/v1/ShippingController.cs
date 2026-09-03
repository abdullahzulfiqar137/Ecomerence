using Asp.Versioning;
using ECommerce.Application.DTOs.Shipping;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shipping")]
public class ShippingController : BaseApiController
{
    private readonly IShippingService _shippingService;
    private readonly IValidator<CreateShippingZoneDto> _zoneValidator;
    private readonly IValidator<CreateShippingMethodDto> _methodValidator;
    private readonly IValidator<CreateShippingRateDto> _rateValidator;
    private readonly IValidator<CreateShipmentDto> _shipmentValidator;
    private readonly IValidator<AddTrackingEventDto> _trackingValidator;

    public ShippingController(
        IShippingService shippingService,
        IValidator<CreateShippingZoneDto> zoneValidator,
        IValidator<CreateShippingMethodDto> methodValidator,
        IValidator<CreateShippingRateDto> rateValidator,
        IValidator<CreateShipmentDto> shipmentValidator,
        IValidator<AddTrackingEventDto> trackingValidator)
    {
        _shippingService = shippingService;
        _zoneValidator = zoneValidator;
        _methodValidator = methodValidator;
        _rateValidator = rateValidator;
        _shipmentValidator = shipmentValidator;
        _trackingValidator = trackingValidator;
    }

    [HttpGet("zones")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ShippingZoneDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetZones(CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetAllZonesAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ShippingManager")]
    [HttpPost("zones")]
    [ProducesResponseType(typeof(ApiResponse<ShippingZoneDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ShippingZoneDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateZone([FromBody] CreateShippingZoneDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _zoneValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ShippingZoneDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _shippingService.CreateZoneAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("methods")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ShippingMethodDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMethods(CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetAllMethodsAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ShippingManager")]
    [HttpPost("methods")]
    [ProducesResponseType(typeof(ApiResponse<ShippingMethodDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMethod([FromBody] CreateShippingMethodDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _methodValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ShippingMethodDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _shippingService.CreateMethodAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ShippingManager")]
    [HttpPost("rates")]
    [ProducesResponseType(typeof(ApiResponse<ShippingRateDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ShippingRateDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRate([FromBody] CreateShippingRateDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _rateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ShippingRateDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _shippingService.CreateRateAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("estimate")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ShippingEstimateResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EstimateRates([FromBody] ShippingEstimateRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _shippingService.EstimateShippingRatesAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ShippingManager")]
    [HttpPost("shipments")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _shipmentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ShipmentDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _shippingService.CreateShipmentAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetShipmentById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("shipments/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetShipmentByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("track/{trackingNumber}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrackShipment(string trackingNumber, CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetShipmentByTrackingNumberAsync(trackingNumber, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ShippingManager")]
    [HttpPost("shipments/{id:guid}/events")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTrackingEvent(Guid id, [FromBody] AddTrackingEventDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _trackingValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ShipmentDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _shippingService.AddTrackingEventAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
