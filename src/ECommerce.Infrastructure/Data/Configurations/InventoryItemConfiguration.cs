using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.RowVersion)
            .IsRowVersion();

        builder.HasOne(ii => ii.Warehouse)
            .WithMany(w => w.InventoryItems)
            .HasForeignKey(ii => ii.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.Product)
            .WithMany(p => p.InventoryItems)
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.ProductVariant)
            .WithMany(pv => pv.InventoryItems)
            .HasForeignKey(ii => ii.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.WarehouseLocation)
            .WithMany()
            .HasForeignKey(ii => ii.WarehouseLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ii => new { ii.WarehouseId, ii.ProductId, ii.ProductVariantId })
            .IsUnique();

        builder.HasQueryFilter(ii => !ii.IsDeleted);
    }
}
