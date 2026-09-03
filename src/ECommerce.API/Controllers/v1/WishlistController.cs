using Asp.Versioning;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wishlist")]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;
    private readonly IValidator<AddToWishlistRequestDto> _addValidator;

    public WishlistController(
        IWishlistService wishlistService,
        IValidator<AddToWishlistRequestDto> addValidator)
    {
        _wishlistService = wishlistService;
        _addValidator = addValidator;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _wishlistService.GetUserWishlistAsync(CurrentUserId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpGet("share/{token}")]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSharedWishlist(string token, CancellationToken cancellationToken)
    {
        var result = await _wishlistService.GetPublicWishlistByTokenAsync(token, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddItem([FromBody] AddToWishlistRequestDto request, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        var validationResult = await _addValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<WishlistDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _wishlistService.AddItemAsync(CurrentUserId.Value, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("items/{wishlistItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveItem(Guid wishlistItemId, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _wishlistService.RemoveItemAsync(CurrentUserId.Value, wishlistItemId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("items/{wishlistItemId:guid}/move-to-cart")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MoveToCart(Guid wishlistItemId, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _wishlistService.MoveToCartAsync(CurrentUserId.Value, wishlistItemId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("share")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateShareToken(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _wishlistService.GenerateShareTokenAsync(CurrentUserId.Value, cancellationToken);
        return Ok(result);
    }
}
