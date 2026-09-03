using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.ToTable("WarehouseLocations");

        builder.HasKey(wl => wl.Id);

        builder.Property(wl => wl.Aisle)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(wl => wl.Rack)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(wl => wl.Shelf)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(wl => wl.Bin)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(wl => wl.Warehouse)
            .WithMany(w => w.Locations)
            .HasForeignKey(wl => wl.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(wl => new { wl.WarehouseId, wl.Aisle, wl.Rack, wl.Shelf, wl.Bin })
            .IsUnique();
    }
}
