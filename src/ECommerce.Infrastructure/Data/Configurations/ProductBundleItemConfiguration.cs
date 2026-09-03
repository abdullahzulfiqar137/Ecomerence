using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductBundleItemConfiguration : IEntityTypeConfiguration<ProductBundleItem>
{
    public void Configure(EntityTypeBuilder<ProductBundleItem> builder)
    {
        builder.ToTable("ProductBundleItems");

        builder.HasKey(pbi => pbi.Id);

        builder.Property(pbi => pbi.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.HasOne(pbi => pbi.BundleProduct)
            .WithMany(p => p.BundleItems)
            .HasForeignKey(pbi => pbi.BundleProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pbi => pbi.ItemProduct)
            .WithMany()
            .HasForeignKey(pbi => pbi.ItemProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pbi => new { pbi.BundleProductId, pbi.ItemProductId });
    }
}
