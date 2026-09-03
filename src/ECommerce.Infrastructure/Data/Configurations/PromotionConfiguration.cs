using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.BannerUrl)
            .HasMaxLength(500);

        builder.Property(p => p.DiscountValue)
            .HasPrecision(18, 2);

        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => new { p.StartsAtUtc, p.EndsAtUtc });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
