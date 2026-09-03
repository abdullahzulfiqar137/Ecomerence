using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable("ShippingMethods");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sm => sm.Code)
            .HasMaxLength(50);

        builder.Property(sm => sm.Description)
            .HasMaxLength(250);

        builder.Property(sm => sm.EstimatedDeliveryDays)
            .HasMaxLength(100);

        builder.HasQueryFilter(sm => !sm.IsDeleted);
    }
}
