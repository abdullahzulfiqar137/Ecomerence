using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.Website)
            .HasMaxLength(250);

        builder.Property(b => b.MetaTitle)
            .HasMaxLength(200);

        builder.Property(b => b.MetaDescription)
            .HasMaxLength(500);

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
