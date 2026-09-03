using System.Security.Cryptography;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Cart;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(ApplicationDbContext context, ICartService cartService, ILogger<WishlistService> logger)
    {
        _context = context;
        _cartService = cartService;
        _logger = logger;
    }

    public async Task<ApiResponse<WishlistDto>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wishlist = await GetOrCreateWishlistEntityAsync(userId, cancellationToken);
        var dto = await MapToWishlistDtoAsync(wishlist, cancellationToken);
        return ApiResponse<WishlistDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<WishlistDto>> GetPublicWishlistByTokenAsync(string shareToken, CancellationToken cancellationToken = default)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.ShareToken == shareToken && w.IsPublic, cancellationToken);

        if (wishlist == null)
        {
            return ApiResponse<WishlistDto>.FailureResponse("Not Found", "Shared wishlist not found or is set to private.");
        }

        var dto = await MapToWishlistDtoAsync(wishlist, cancellationToken);
        return ApiResponse<WishlistDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<WishlistDto>> AddItemAsync(Guid userId, AddToWishlistRequestDto request, CancellationToken cancellationToken = default)
    {
        var wishlist = await GetOrCreateWishlistEntityAsync(userId, cancellationToken);

        var exists = wishlist.Items.Any(i => i.ProductId == request.ProductId && i.ProductVariantId == request.ProductVariantId);
        if (!exists)
        {
            await _context.WishlistItems.AddAsync(new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = request.ProductId,
                ProductVariantId = request.ProductVariantId,
                AddedAtUtc = DateTime.UtcNow
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = await MapToWishlistDtoAsync(wishlist, cancellationToken);
        return ApiResponse<WishlistDto>.SuccessResponse(dto, "Item added to wishlist.");
    }

    public async Task<ApiResponse<WishlistDto>> RemoveItemAsync(Guid userId, Guid wishlistItemId, CancellationToken cancellationToken = default)
    {
        var wishlist = await GetOrCreateWishlistEntityAsync(userId, cancellationToken);
        var item = wishlist.Items.FirstOrDefault(i => i.Id == wishlistItemId);

        if (item != null)
        {
            wishlist.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = await MapToWishlistDtoAsync(wishlist, cancellationToken);
        return ApiResponse<WishlistDto>.SuccessResponse(dto, "Item removed from wishlist.");
    }

    public async Task<ApiResponse<CartDto>> MoveToCartAsync(Guid userId, Guid wishlistItemId, CancellationToken cancellationToken = default)
    {
        var wishlist = await GetOrCreateWishlistEntityAsync(userId, cancellationToken);
        var item = wishlist.Items.FirstOrDefault(i => i.Id == wishlistItemId);

        if (item == null)
        {
            return ApiResponse<CartDto>.FailureResponse("Not Found", "Wishlist item not found.");
        }

        // 1. Add to cart
        var addResult = await _cartService.AddItemAsync(userId, new AddToCartRequestDto
        {
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            Quantity = 1
        }, cancellationToken);

        if (!addResult.Success)
        {
            return addResult;
        }

        // 2. Remove from wishlist
        wishlist.Items.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return addResult;
    }

    public async Task<ApiResponse<string>> GenerateShareTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wishlist = await GetOrCreateWishlistEntityAsync(userId, cancellationToken);

        var tokenBytes = RandomNumberGenerator.GetBytes(16);
        wishlist.ShareToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();
        wishlist.IsPublic = true;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.SuccessResponse(wishlist.ShareToken, "Share link generated successfully.");
    }

    private async Task<Domain.Entities.Wishlist> GetOrCreateWishlistEntityAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wishlist == null)
        {
            wishlist = new Domain.Entities.Wishlist
            {
                UserId = userId,
                Name = "My Wishlist",
                IsPublic = false
            };
            await _context.Wishlists.AddAsync(wishlist, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return wishlist;
    }

    private async Task<WishlistDto> MapToWishlistDtoAsync(Domain.Entities.Wishlist wishlist, CancellationToken cancellationToken)
    {
        var productIds = wishlist.Items.Select(i => i.ProductId).Distinct().ToList();
        var variantIds = wishlist.Items.Where(i => i.ProductVariantId.HasValue).Select(i => i.ProductVariantId!.Value).Distinct().ToList();

        var products = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.InventoryItems)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var variants = await _context.ProductVariants
            .Include(v => v.InventoryItems)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var itemDtos = new List<WishlistItemDto>();

        foreach (var item in wishlist.Items)
        {
            products.TryGetValue(item.ProductId, out var product);
            ProductVariant? variant = null;
            if (item.ProductVariantId.HasValue)
            {
                variants.TryGetValue(item.ProductVariantId.Value, out variant);
            }

            var price = variant?.Price ?? product?.Price ?? 0m;
            var salePrice = variant?.SalePrice ?? product?.SalePrice;
            var stock = variant != null
                ? variant.InventoryItems.Sum(i => i.QuantityAvailable)
                : (product?.InventoryItems.Sum(i => i.QuantityAvailable) ?? 0);

            var mainImage = variant?.ImageUrl
                ?? product?.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl
                ?? product?.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;

            itemDtos.Add(new WishlistItemDto
            {
                Id = item.Id,
                WishlistId = item.WishlistId,
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Product",
                ProductSlug = product?.Slug ?? string.Empty,
                ProductSKU = product?.SKU ?? string.Empty,
                Price = price,
                SalePrice = salePrice,
                ImageUrl = mainImage,
                ProductVariantId = item.ProductVariantId,
                VariantName = variant?.Name,
                VariantSKU = variant?.SKU,
                IsInStock = stock > 0,
                AddedAtUtc = item.AddedAtUtc
            });
        }

        return new WishlistDto
        {
            Id = wishlist.Id,
            UserId = wishlist.UserId,
            Name = wishlist.Name,
            IsPublic = wishlist.IsPublic,
            ShareToken = wishlist.ShareToken,
            Items = itemDtos
        };
    }
}
