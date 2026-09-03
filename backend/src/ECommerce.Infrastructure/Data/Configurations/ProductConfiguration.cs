using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(1000);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.SalePrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.TaxRate)
            .HasPrecision(18, 4);

        builder.Property(p => p.AverageRating)
            .HasPrecision(3, 2);

        builder.Property(p => p.Weight)
            .HasPrecision(18, 3);

        builder.Property(p => p.Length)
            .HasPrecision(18, 2);

        builder.Property(p => p.Width)
            .HasPrecision(18, 2);

        builder.Property(p => p.Height)
            .HasPrecision(18, 2);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(200);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.MetaKeywords)
            .HasMaxLength(300);

        builder.Property(p => p.CanonicalUrl)
            .HasMaxLength(500);

        // Indexes for scalable queries over millions of records
        builder.HasIndex(p => p.SKU)
            .IsUnique();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BrandId);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.CreatedAtUtc);
        builder.HasIndex(p => p.Price);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
