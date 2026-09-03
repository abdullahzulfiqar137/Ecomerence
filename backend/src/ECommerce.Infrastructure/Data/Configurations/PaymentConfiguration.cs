using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ExternalTransactionId)
            .HasMaxLength(150);

        builder.Property(p => p.IdempotencyKey)
            .HasMaxLength(100);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.RawGatewayResponse)
            .HasMaxLength(4000);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.ExternalTransactionId);
        builder.HasIndex(p => p.IdempotencyKey);
        builder.HasIndex(p => p.Status);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
