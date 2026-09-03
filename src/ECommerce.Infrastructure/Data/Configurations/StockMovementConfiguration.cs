using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.ReferenceType)
            .HasMaxLength(100);

        builder.Property(sm => sm.ReferenceId)
            .HasMaxLength(100);

        builder.Property(sm => sm.Notes)
            .HasMaxLength(500);

        builder.Property(sm => sm.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(sm => sm.InventoryItem)
            .WithMany(ii => ii.StockMovements)
            .HasForeignKey(sm => sm.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sm => sm.InventoryItemId);
        builder.HasIndex(sm => sm.CreatedAtUtc);
        builder.HasIndex(sm => new { sm.ReferenceType, sm.ReferenceId });
    }
}
