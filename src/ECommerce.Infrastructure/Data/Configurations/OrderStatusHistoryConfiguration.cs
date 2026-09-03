using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(osh => osh.Id);

        builder.Property(osh => osh.Comment)
            .HasMaxLength(500);

        builder.Property(osh => osh.ChangedBy)
            .HasMaxLength(100);

        builder.HasOne(osh => osh.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(osh => osh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(osh => osh.OrderId);
        builder.HasIndex(osh => osh.CreatedAtUtc);
    }
}
