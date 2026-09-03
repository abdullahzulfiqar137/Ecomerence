using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync(Guid? userId, string? guestSessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> AddItemAsync(Guid? userId, AddToCartRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> UpdateItemQuantityAsync(Guid? userId, string? guestSessionId, Guid cartItemId, int quantity, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> RemoveItemAsync(Guid? userId, string? guestSessionId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> ToggleSaveForLaterAsync(Guid? userId, string? guestSessionId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> MergeCartAsync(Guid userId, string guestSessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ClearCartAsync(Guid? userId, string? guestSessionId, CancellationToken cancellationToken = default);
}

public interface IWishlistService
{
    Task<ApiResponse<WishlistDto>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<WishlistDto>> GetPublicWishlistByTokenAsync(string shareToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<WishlistDto>> AddItemAsync(Guid userId, AddToWishlistRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<WishlistDto>> RemoveItemAsync(Guid userId, Guid wishlistItemId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CartDto>> MoveToCartAsync(Guid userId, Guid wishlistItemId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GenerateShareTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}
