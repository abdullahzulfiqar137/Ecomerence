using Asp.Versioning;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;
    private readonly IValidator<AddToCartRequestDto> _addValidator;
    private readonly IValidator<UpdateCartItemRequestDto> _updateValidator;

    public CartController(
        ICartService cartService,
        IValidator<AddToCartRequestDto> addValidator,
        IValidator<UpdateCartItemRequestDto> updateValidator)
    {
        _cartService = cartService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart([FromQuery] string? guestSessionId, CancellationToken cancellationToken)
    {
        var result = await _cartService.GetCartAsync(CurrentUserId, guestSessionId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _addValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CartDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _cartService.AddItemAsync(CurrentUserId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity(Guid cartItemId, [FromBody] UpdateCartItemRequestDto request, [FromQuery] string? guestSessionId, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CartDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _cartService.UpdateItemQuantityAsync(CurrentUserId, guestSessionId, cartItemId, request.Quantity, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, [FromQuery] string? guestSessionId, CancellationToken cancellationToken)
    {
        var result = await _cartService.RemoveItemAsync(CurrentUserId, guestSessionId, cartItemId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("items/{cartItemId:guid}/save-for-later")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleSaveForLater(Guid cartItemId, [FromQuery] string? guestSessionId, CancellationToken cancellationToken)
    {
        var result = await _cartService.ToggleSaveForLaterAsync(CurrentUserId, guestSessionId, cartItemId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("merge")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MergeCart([FromBody] MergeCartRequestDto request, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized(ApiResponse<CartDto>.FailureResponse("Unauthorized", "User must be authenticated to merge cart."));
        }

        var result = await _cartService.MergeCartAsync(CurrentUserId.Value, request.GuestSessionId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("clear")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart([FromQuery] string? guestSessionId, CancellationToken cancellationToken)
    {
        var result = await _cartService.ClearCartAsync(CurrentUserId, guestSessionId, cancellationToken);
        return Ok(result);
    }
}
