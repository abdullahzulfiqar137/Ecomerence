using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CarrierName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.TrackingUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        builder.Property(s => s.ShippingCost)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.TrackingNumber);
        builder.HasIndex(s => s.Status);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
