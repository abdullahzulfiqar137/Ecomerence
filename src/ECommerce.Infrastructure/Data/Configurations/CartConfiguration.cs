using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.GuestSessionId)
            .HasMaxLength(100);

        builder.Property(c => c.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.AppliedCouponCode)
            .HasMaxLength(50);

        builder.Property(c => c.DiscountAmount)
            .HasPrecision(18, 2);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.GuestSessionId);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
