using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Cart;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CartService> _logger;

    public CartService(ApplicationDbContext context, ILogger<CartService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<CartDto>> GetCartAsync(Guid? userId, string? guestSessionId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartEntityAsync(userId, guestSessionId, cancellationToken);
        var dto = await MapToCartDtoAsync(cart, cancellationToken);
        return ApiResponse<CartDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<CartDto>> AddItemAsync(Guid? userId, AddToCartRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartEntityAsync(userId, request.GuestSessionId, cancellationToken);

        // Fetch product and variant to check validity and price
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive, cancellationToken);

        if (product == null)
        {
            return ApiResponse<CartDto>.FailureResponse("Not Found", "Product not found or inactive.");
        }

        decimal itemPrice = product.SalePrice ?? product.Price;
        if (request.ProductVariantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == request.ProductVariantId.Value && v.IsActive);
            if (variant == null)
            {
                return ApiResponse<CartDto>.FailureResponse("Not Found", "Product variant not found or inactive.");
            }
            itemPrice = variant.SalePrice ?? variant.Price;
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId &&
            i.ProductVariantId == request.ProductVariantId &&
            !i.IsSavedForLater);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = itemPrice; // update to current price
            existingItem.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                UnitPrice = itemPrice,
                IsSavedForLater = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _context.CartItems.AddAsync(cartItem, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added product {ProductId} (variant: {VariantId}) x{Qty} to cart {CartId}",
            request.ProductId, request.ProductVariantId, request.Quantity, cart.Id);

        var dto = await MapToCartDtoAsync(cart, cancellationToken);
        return ApiResponse<CartDto>.SuccessResponse(dto, "Item added to cart successfully.");
    }

    public async Task<ApiResponse<CartDto>> UpdateItemQuantityAsync(Guid? userId, string? guestSessionId, Guid cartItemId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartEntityAsync(userId, guestSessionId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (item == null)
        {
            return ApiResponse<CartDto>.FailureResponse("Not Found", "Cart item not found.");
        }

        if (quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
            item.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await MapToCartDtoAsync(cart, cancellationToken);
        return ApiResponse<CartDto>.SuccessResponse(dto, "Cart updated successfully.");
    }

    public async Task<ApiResponse<CartDto>> RemoveItemAsync(Guid? userId, string? guestSessionId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartEntityAsync(userId, guestSessionId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (item == null)
        {
            return ApiResponse<CartDto>.FailureResponse("Not Found", "Cart item not found.");
        }

        cart.Items.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = await MapToCartDtoAsync(cart, cancellationToken);
        return ApiResponse<CartDto>.SuccessResponse(dto, "Item removed from cart.");
    }

    public async Task<ApiResponse<CartDto>> ToggleSaveForLaterAsync(Guid? userId, string? guestSessionId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartEntityAsync(userId, guestSessionId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (item == null)
        {
            return ApiResponse<CartDto>.FailureResponse("Not Found", "Cart item not found.");
        }

        item.IsSavedForLater = !item.IsSavedForLater;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var message = item.IsSavedForLater ? "Item saved for later." : "Item moved back to cart.";
        var dto = await MapToCartDtoAsync(cart, cancellationToken);
        return ApiResponse<CartDto>.SuccessResponse(dto, message);
    }

    public async Task<ApiResponse<CartDto>> MergeCartAsync(Guid userId, string guestSessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guestSessionId))
        {
            return await GetCartAsync(userId, null, cancellationToken);
        }

        var guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.GuestSessionId == guestSessionId && c.UserId == null, cancellationToken);

        var userCart = await GetOrCreateCartEntityAsync(userId, null, cancellationToken);

        if (guestCart != null && guestCart.Items.Any())
        {
            foreach (var guestItem in guestCart.Items.ToList())
            {
                var existingUserItem = userCart.Items.FirstOrDefault(i =>
                    i.ProductId == guestItem.ProductId &&
                    i.ProductVariantId == guestItem.ProductVariantId &&
                    i.IsSavedForLater == guestItem.IsSavedForLater);

                if (existingUserItem != null)
                {
                    existingUserItem.Quantity += guestItem.Quantity;
                    existingUserItem.UpdatedAtUtc = DateTime.UtcNow;
                }
                else
                {
                    await _context.CartItems.AddAsync(new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = guestItem.ProductId,
                        ProductVariantId = guestItem.ProductVariantId,
                        Quantity = guestItem.Quantity,
                        UnitPrice = guestItem.UnitPrice,
                        IsSavedForLater = guestItem.IsSavedForLater,
                        CreatedAtUtc = DateTime.UtcNow
                    }, cancellationToken);
                }
            }

            // Remove guest cart
            _context.Carts.Remove(guestCart);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Merged guest cart {GuestSessionId} into user cart {UserId}", guestSessionId, userId);
        }

        var dto = await MapToCartDtoAsync(userCart, cancellationToken);
        return ApiResponse<CartDto>.SuccessResponse(dto, "Cart merged successfully.");
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(Guid? userId, string? guestSessionId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartEntityAsync(userId, guestSessionId, cancellationToken);
        cart.Items.Clear();
        cart.AppliedCouponCode = null;
        cart.DiscountAmount = 0m;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Cart cleared successfully.");
    }

    private async Task<Domain.Entities.Cart> GetOrCreateCartEntityAsync(Guid? userId, string? guestSessionId, CancellationToken cancellationToken)
    {
        Domain.Entities.Cart? cart = null;

        if (userId.HasValue)
        {
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(guestSessionId))
        {
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.GuestSessionId == guestSessionId, cancellationToken);
        }

        if (cart == null)
        {
            cart = new Domain.Entities.Cart
            {
                UserId = userId,
                GuestSessionId = userId.HasValue ? null : (guestSessionId ?? Guid.NewGuid().ToString("N")),
                Currency = "USD",
                ExpiresAtUtc = userId.HasValue ? null : DateTime.UtcNow.AddDays(30)
            };
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }

    private async Task<CartDto> MapToCartDtoAsync(Domain.Entities.Cart cart, CancellationToken cancellationToken)
    {
        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var variantIds = cart.Items.Where(i => i.ProductVariantId.HasValue).Select(i => i.ProductVariantId!.Value).Distinct().ToList();

        var products = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.InventoryItems)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var variants = await _context.ProductVariants
            .Include(v => v.InventoryItems)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var activeItems = new List<CartItemDto>();
        var savedItems = new List<CartItemDto>();

        foreach (var item in cart.Items)
        {
            products.TryGetValue(item.ProductId, out var product);
            ProductVariant? variant = null;
            if (item.ProductVariantId.HasValue)
            {
                variants.TryGetValue(item.ProductVariantId.Value, out variant);
            }

            var stock = variant != null
                ? variant.InventoryItems.Sum(i => i.QuantityAvailable)
                : (product?.InventoryItems.Sum(i => i.QuantityAvailable) ?? 0);

            var mainImage = variant?.ImageUrl
                ?? product?.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl
                ?? product?.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;

            var itemDto = new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Product",
                ProductSlug = product?.Slug ?? string.Empty,
                ProductSKU = product?.SKU ?? string.Empty,
                ProductVariantId = item.ProductVariantId,
                VariantName = variant?.Name,
                VariantSKU = variant?.SKU,
                ImageUrl = mainImage,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                IsInStock = stock >= item.Quantity,
                MaxAvailableStock = stock,
                IsSavedForLater = item.IsSavedForLater
            };

            if (item.IsSavedForLater)
            {
                savedItems.Add(itemDto);
            }
            else
            {
                activeItems.Add(itemDto);
            }
        }

        var subtotal = activeItems.Sum(i => i.TotalPrice);

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            GuestSessionId = cart.GuestSessionId,
            Currency = cart.Currency,
            AppliedCouponCode = cart.AppliedCouponCode,
            SubtotalAmount = subtotal,
            DiscountAmount = cart.DiscountAmount,
            Items = activeItems,
            SavedForLaterItems = savedItems
        };
    }
}
