using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("ProductSpecifications");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Group)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ps => ps.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(ps => ps.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(ps => ps.Product)
            .WithMany(p => p.Specifications)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ps => ps.ProductId);
    }
}
