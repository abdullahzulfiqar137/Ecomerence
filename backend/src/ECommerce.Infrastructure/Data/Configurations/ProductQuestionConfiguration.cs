using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductQuestionConfiguration : IEntityTypeConfiguration<ProductQuestion>
{
    public void Configure(EntityTypeBuilder<ProductQuestion> builder)
    {
        builder.ToTable("ProductQuestions");

        builder.HasKey(pq => pq.Id);

        builder.Property(pq => pq.QuestionText)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(pq => pq.Product)
            .WithMany(p => p.Questions)
            .HasForeignKey(pq => pq.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pq => pq.User)
            .WithMany()
            .HasForeignKey(pq => pq.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pq => pq.ProductId);
        builder.HasQueryFilter(pq => !pq.IsDeleted);
    }
}
