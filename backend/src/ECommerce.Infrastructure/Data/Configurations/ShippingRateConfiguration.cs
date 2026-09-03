using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
{
    public void Configure(EntityTypeBuilder<ShippingRate> builder)
    {
        builder.ToTable("ShippingRates");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.BaseRate)
            .HasPrecision(18, 2);

        builder.Property(sr => sr.RatePerKg)
            .HasPrecision(18, 2);

        builder.Property(sr => sr.FreeShippingThreshold)
            .HasPrecision(18, 2);

        builder.Property(sr => sr.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(sr => sr.ShippingZone)
            .WithMany(sz => sz.ShippingRates)
            .HasForeignKey(sr => sr.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.ShippingMethod)
            .WithMany(sm => sm.ShippingRates)
            .HasForeignKey(sr => sr.ShippingMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sr => new { sr.ShippingZoneId, sr.ShippingMethodId });
    }
}
