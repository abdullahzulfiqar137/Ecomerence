using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductVideoConfiguration : IEntityTypeConfiguration<ProductVideo>
{
    public void Configure(EntityTypeBuilder<ProductVideo> builder)
    {
        builder.ToTable("ProductVideos");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.VideoUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pv => pv.Title)
            .HasMaxLength(200);

        builder.Property(pv => pv.ThumbnailUrl)
            .HasMaxLength(500);

        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Videos)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pv => pv.ProductId);
    }
}
