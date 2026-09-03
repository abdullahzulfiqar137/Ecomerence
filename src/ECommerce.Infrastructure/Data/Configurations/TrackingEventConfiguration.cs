using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.ToTable("TrackingEvents");

        builder.HasKey(te => te.Id);

        builder.Property(te => te.Location)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(te => te.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(te => te.Shipment)
            .WithMany(s => s.TrackingEvents)
            .HasForeignKey(te => te.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(te => te.ShipmentId);
        builder.HasIndex(te => te.EventTimeUtc);
    }
}
