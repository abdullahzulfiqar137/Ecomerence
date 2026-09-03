using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.IdempotencyKey)
            .HasMaxLength(100);

        builder.Property(o => o.CustomerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.CustomerPhone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(o => o.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(o => o.AppliedCouponCode)
            .HasMaxLength(50);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        // Monetary Precisions
        builder.Property(o => o.ExchangeRate)
            .HasPrecision(18, 6);

        builder.Property(o => o.SubtotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        // Critical indexes
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.HasIndex(o => o.IdempotencyKey);
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);
        builder.HasIndex(o => o.ShippingStatus);
        builder.HasIndex(o => o.CreatedAtUtc);

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
