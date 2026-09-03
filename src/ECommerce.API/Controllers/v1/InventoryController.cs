using Asp.Versioning;
using ECommerce.Application.DTOs.Inventory;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/inventory")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;
    private readonly IValidator<AdjustStockRequestDto> _adjustValidator;
    private readonly IValidator<CreateReservationRequestDto> _reserveValidator;

    public InventoryController(
        IInventoryService inventoryService,
        IValidator<AdjustStockRequestDto> adjustValidator,
        IValidator<CreateReservationRequestDto> reserveValidator)
    {
        _inventoryService = inventoryService;
        _adjustValidator = adjustValidator;
        _reserveValidator = reserveValidator;
    }

    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventory([FromQuery] InventoryFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetInventoryAsync(filterParams, cancellationToken);
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId:guid}/product/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStock(Guid warehouseId, Guid productId, [FromQuery] Guid? variantId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetStockByWarehouseAndProductAsync(warehouseId, productId, variantId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _adjustValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<InventoryItemDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _inventoryService.AdjustStockAsync(request, CurrentUserEmail, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("reserve")]
    [ProducesResponseType(typeof(ApiResponse<InventoryReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InventoryReservationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReserveStock([FromBody] CreateReservationRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _reserveValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<InventoryReservationDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _inventoryService.ReserveStockAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("release/{reservationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReleaseReservation(Guid reservationId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.ReleaseReservationAsync(reservationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("commit/{reservationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CommitReservation(Guid reservationId, [FromQuery] string? orderNumber, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CommitReservationAsync(reservationId, orderNumber, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LowStockAlertDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockAlerts(CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetLowStockAlertsAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
    [HttpGet("item/{inventoryItemId:guid}/movements")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockMovements(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetStockMovementsAsync(inventoryItemId, cancellationToken);
        return Ok(result);
    }
}
