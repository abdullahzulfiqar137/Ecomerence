using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(wi => wi.Id);

        builder.HasOne(wi => wi.Wishlist)
            .WithMany(w => w.Items)
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wi => wi.Product)
            .WithMany()
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wi => wi.ProductVariant)
            .WithMany()
            .HasForeignKey(wi => wi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wi => new { wi.WishlistId, wi.ProductId, wi.ProductVariantId });
    }
}
