using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("ProductAttributeValues");

        builder.HasKey(pav => pav.Id);

        builder.Property(pav => pav.Value)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(pav => pav.ProductAttribute)
            .WithMany(pa => pa.Values)
            .HasForeignKey(pav => pav.ProductAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pav => pav.ProductVariant)
            .WithMany(pv => pv.AttributeValues)
            .HasForeignKey(pav => pav.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pav => new { pav.ProductAttributeId, pav.ProductVariantId });
    }
}
