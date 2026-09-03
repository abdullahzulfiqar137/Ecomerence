using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(250);

        builder.Property(c => c.DiscountValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.MaximumDiscountAmount)
            .HasPrecision(18, 2);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
