using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsages");

        builder.HasKey(cu => cu.Id);

        builder.Property(cu => cu.DiscountApplied)
            .HasPrecision(18, 2);

        builder.HasOne(cu => cu.Coupon)
            .WithMany(c => c.Usages)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cu => cu.Order)
            .WithMany()
            .HasForeignKey(cu => cu.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cu => cu.User)
            .WithMany()
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cu => cu.CouponId);
        builder.HasIndex(cu => cu.UserId);
        builder.HasIndex(cu => cu.OrderId);
    }
}
