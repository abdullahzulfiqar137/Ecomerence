using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pv => pv.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pv => pv.Barcode)
            .HasMaxLength(100);

        builder.Property(pv => pv.Size)
            .HasMaxLength(50);

        builder.Property(pv => pv.Color)
            .HasMaxLength(50);

        builder.Property(pv => pv.ColorCode)
            .HasMaxLength(20);

        builder.Property(pv => pv.ImageUrl)
            .HasMaxLength(500);

        builder.Property(pv => pv.Price)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.SalePrice)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.Weight)
            .HasPrecision(18, 3);

        builder.Property(pv => pv.Length)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.Width)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.Height)
            .HasPrecision(18, 2);

        builder.HasIndex(pv => pv.SKU)
            .IsUnique();

        builder.HasIndex(pv => pv.ProductId);

        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(pv => !pv.IsDeleted);
    }
}
