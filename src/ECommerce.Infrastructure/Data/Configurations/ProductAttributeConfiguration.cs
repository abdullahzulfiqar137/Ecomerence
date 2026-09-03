using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pa => pa.Description)
            .HasMaxLength(250);

        builder.HasQueryFilter(pa => !pa.IsDeleted);
    }
}
