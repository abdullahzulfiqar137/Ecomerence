using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class DiscountRuleConfiguration : IEntityTypeConfiguration<DiscountRule>
{
    public void Configure(EntityTypeBuilder<DiscountRule> builder)
    {
        builder.ToTable("DiscountRules");

        builder.HasKey(dr => dr.Id);

        builder.Property(dr => dr.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(dr => dr.Value)
            .HasPrecision(18, 2);

        builder.Property(dr => dr.MinSubtotal)
            .HasPrecision(18, 2);

        builder.Property(dr => dr.MaxDiscount)
            .HasPrecision(18, 2);

        builder.HasQueryFilter(dr => !dr.IsDeleted);
    }
}
