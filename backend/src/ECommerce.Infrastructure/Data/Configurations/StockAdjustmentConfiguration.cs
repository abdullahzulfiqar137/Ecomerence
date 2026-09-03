using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Reason)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(sa => sa.ApprovedBy)
            .HasMaxLength(100);

        builder.HasOne(sa => sa.InventoryItem)
            .WithMany()
            .HasForeignKey(sa => sa.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sa => sa.InventoryItemId);
        builder.HasQueryFilter(sa => !sa.IsDeleted);
    }
}
