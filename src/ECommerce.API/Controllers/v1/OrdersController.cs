using Asp.Versioning;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IValidator<UpdateOrderStatusDto> _updateStatusValidator;
    private readonly IValidator<CancelOrderRequestDto> _cancelValidator;
    private readonly IValidator<AddOrderNoteDto> _noteValidator;

    public OrdersController(
        IOrderService orderService,
        IValidator<CreateOrderDto> createValidator,
        IValidator<UpdateOrderStatusDto> updateStatusValidator,
        IValidator<CancelOrderRequestDto> cancelValidator,
        IValidator<AddOrderNoteDto> noteValidator)
    {
        _orderService = orderService;
        _createValidator = createValidator;
        _updateStatusValidator = updateStatusValidator;
        _cancelValidator = cancelValidator;
        _noteValidator = noteValidator;
    }

    [Authorize(Roles = "SuperAdmin,Admin,OrderManager")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] OrderFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrdersAsync(filterParams, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] OrderFilterParams filterParams, CancellationToken cancellationToken)
    {
        filterParams.CustomerUserId = CurrentUserId;
        var result = await _orderService.GetOrdersAsync(filterParams, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("OrderManager");
        var result = await _orderService.GetOrderByIdAsync(id, CurrentUserId, isAdmin, cancellationToken);
        if (!result.Success)
        {
            return result.Message == "Forbidden" ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("OrderManager");
        var result = await _orderService.GetOrderByNumberAsync(orderNumber, CurrentUserId, isAdmin, cancellationToken);
        if (!result.Success)
        {
            return result.Message == "Forbidden" ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<OrderDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _orderService.CreateOrderAsync(CurrentUserId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,OrderManager")]
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<OrderDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _orderService.UpdateOrderStatusAsync(id, request, CurrentUserEmail, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _cancelValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<OrderDto>.FailureResponse("Validation failed", errors));
        }

        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("OrderManager");
        var result = await _orderService.CancelOrderAsync(id, request, CurrentUserId, isAdmin, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,OrderManager")]
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(typeof(ApiResponse<OrderNoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderNoteDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddOrderNoteDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _noteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<OrderNoteDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _orderService.AddOrderNoteAsync(id, request, CurrentUserEmail, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
