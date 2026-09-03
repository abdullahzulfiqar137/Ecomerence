using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pr => pr.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(pr => pr.AdminResponse)
            .HasMaxLength(1000);

        builder.HasOne(pr => pr.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pr => pr.ProductId);
        builder.HasIndex(pr => pr.UserId);
        builder.HasIndex(pr => pr.Status);

        builder.HasQueryFilter(pr => !pr.IsDeleted);
    }
}
