using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class IdempotentRequestConfiguration : IEntityTypeConfiguration<IdempotentRequest>
{
    public void Configure(EntityTypeBuilder<IdempotentRequest> builder)
    {
        builder.ToTable("IdempotentRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Method)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.RequestHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ResponseBody)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
