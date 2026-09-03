using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("ProductTags");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Tag)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(pt => pt.Product)
            .WithMany(p => p.Tags)
            .HasForeignKey(pt => pt.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pt => pt.Tag);
        builder.HasIndex(pt => pt.ProductId);
    }
}
