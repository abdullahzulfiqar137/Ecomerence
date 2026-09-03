using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class LowStockAlertConfiguration : IEntityTypeConfiguration<LowStockAlert>
{
    public void Configure(EntityTypeBuilder<LowStockAlert> builder)
    {
        builder.ToTable("LowStockAlerts");

        builder.HasKey(lsa => lsa.Id);

        builder.HasOne(lsa => lsa.InventoryItem)
            .WithMany()
            .HasForeignKey(lsa => lsa.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(lsa => lsa.InventoryItemId);
        builder.HasIndex(lsa => lsa.IsResolved);
    }
}
