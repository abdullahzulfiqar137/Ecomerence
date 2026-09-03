namespace ECommerce.Application.DTOs.Wishlist;

public class WishlistDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string? ShareToken { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid WishlistId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string? VariantName { get; set; }
    public string? VariantSKU { get; set; }
    public bool IsInStock { get; set; }
    public DateTime AddedAtUtc { get; set; }
}

public class AddToWishlistRequestDto
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
}
