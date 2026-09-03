using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductAnswerConfiguration : IEntityTypeConfiguration<ProductAnswer>
{
    public void Configure(EntityTypeBuilder<ProductAnswer> builder)
    {
        builder.ToTable("ProductAnswers");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.AnswerText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(pa => pa.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(pa => pa.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.User)
            .WithMany()
            .HasForeignKey(pa => pa.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pa => pa.QuestionId);
        builder.HasQueryFilter(pa => !pa.IsDeleted);
    }
}
