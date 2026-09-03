using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");

        builder.HasKey(sz => sz.Id);

        builder.Property(sz => sz.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sz => sz.Description)
            .HasMaxLength(250);

        builder.Property(sz => sz.CountryCodes)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasQueryFilter(sz => !sz.IsDeleted);
    }
}
