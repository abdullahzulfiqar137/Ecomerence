using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Cart;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class CartAndWishlistTests
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;
    private readonly WishlistService _wishlistService;

    public CartAndWishlistTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _cartService = new CartService(_context, NullLogger<CartService>.Instance);
        _wishlistService = new WishlistService(_context, _cartService, NullLogger<WishlistService>.Instance);
    }

    [Fact]
    public async Task AddItemToGuestCart_ShouldCreateCartAndCalculateSubtotal()
    {
        var product = await _context.Products.FirstAsync(p => p.Price > 0);
        var guestSession = Guid.NewGuid().ToString("N");

        var addRequest = new AddToCartRequestDto
        {
            ProductId = product.Id,
            Quantity = 2,
            GuestSessionId = guestSession
        };

        var response = await _cartService.AddItemAsync(null, addRequest);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCount(1);
        response.Data.TotalItems.Should().Be(2);

        var expectedUnitPrice = product.SalePrice ?? product.Price;
        response.Data.SubtotalAmount.Should().Be(expectedUnitPrice * 2);
    }

    [Fact]
    public async Task MergeGuestCart_IntoUserCart_ShouldCombineItemsCorrectly()
    {
        var user = await _context.Users.FirstAsync();
        var product1 = await _context.Products.FirstAsync();
        var product2 = await _context.Products.Skip(1).FirstAsync();

        var guestSession = Guid.NewGuid().ToString("N");

        // 1. Add item 1 to guest cart (qty: 2)
        await _cartService.AddItemAsync(null, new AddToCartRequestDto
        {
            ProductId = product1.Id,
            Quantity = 2,
            GuestSessionId = guestSession
        });

        // 2. Add item 1 (qty: 1) and item 2 (qty: 3) to user cart
        await _cartService.AddItemAsync(user.Id, new AddToCartRequestDto
        {
            ProductId = product1.Id,
            Quantity = 1
        });
        await _cartService.AddItemAsync(user.Id, new AddToCartRequestDto
        {
            ProductId = product2.Id,
            Quantity = 3
        });

        // 3. Merge guest cart into user cart
        var mergeResponse = await _cartService.MergeCartAsync(user.Id, guestSession);

        mergeResponse.Success.Should().BeTrue();
        mergeResponse.Data.Should().NotBeNull();
        mergeResponse.Data!.Items.Should().HaveCount(2);

        var mergedItem1 = mergeResponse.Data.Items.First(i => i.ProductId == product1.Id);
        mergedItem1.Quantity.Should().Be(3); // 2 from guest + 1 from user

        var mergedItem2 = mergeResponse.Data.Items.First(i => i.ProductId == product2.Id);
        mergedItem2.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task Wishlist_Add_And_MoveToCart_ShouldSucceed()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        // 1. Add to wishlist
        var addResponse = await _wishlistService.AddItemAsync(user.Id, new AddToWishlistRequestDto
        {
            ProductId = product.Id
        });

        addResponse.Success.Should().BeTrue();
        addResponse.Data!.Items.Should().HaveCount(1);
        var wishlistItemId = addResponse.Data.Items[0].Id;

        // 2. Move to cart
        var moveResponse = await _wishlistService.MoveToCartAsync(user.Id, wishlistItemId);

        moveResponse.Success.Should().BeTrue();
        moveResponse.Data!.Items.Should().HaveCount(1);

        // Verify wishlist is now empty
        var wishlist = await _wishlistService.GetUserWishlistAsync(user.Id);
        wishlist.Data!.Items.Should().BeEmpty();
    }
}
