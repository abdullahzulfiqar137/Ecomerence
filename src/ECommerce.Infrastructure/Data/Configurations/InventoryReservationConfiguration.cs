using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.ToTable("InventoryReservations");

        builder.HasKey(ir => ir.Id);

        builder.Property(ir => ir.CartSessionId)
            .HasMaxLength(100);

        builder.HasOne(ir => ir.InventoryItem)
            .WithMany(ii => ii.Reservations)
            .HasForeignKey(ir => ir.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ir => ir.Order)
            .WithMany(o => o.Reservations)
            .HasForeignKey(ir => ir.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ir => ir.InventoryItemId);
        builder.HasIndex(ir => ir.OrderId);
        builder.HasIndex(ir => ir.ExpiresAtUtc);
        builder.HasIndex(ir => new { ir.IsReleased, ir.IsCommitted });
    }
}
