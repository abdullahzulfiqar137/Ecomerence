using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductRelatedProductConfiguration : IEntityTypeConfiguration<ProductRelatedProduct>
{
    public void Configure(EntityTypeBuilder<ProductRelatedProduct> builder)
    {
        builder.ToTable("ProductRelatedProducts");

        builder.HasKey(pr => new { pr.ProductId, pr.RelatedProductId });

        builder.HasOne(pr => pr.Product)
            .WithMany(p => p.RelatedProducts)
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.RelatedProduct)
            .WithMany()
            .HasForeignKey(pr => pr.RelatedProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
