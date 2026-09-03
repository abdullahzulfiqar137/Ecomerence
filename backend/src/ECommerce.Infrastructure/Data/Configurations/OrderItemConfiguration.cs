using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(oi => oi.VariantName)
            .HasMaxLength(200);

        builder.Property(oi => oi.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.ImageUrl)
            .HasMaxLength(500);

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TotalLineAmount)
            .HasPrecision(18, 2);

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
    }
}
