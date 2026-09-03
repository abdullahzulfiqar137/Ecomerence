using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("CustomerProfiles");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TotalSpent)
            .HasPrecision(18, 2);

        builder.Property(c => c.PreferredCurrency)
            .HasMaxLength(10);

        builder.Property(c => c.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(c => c.Gender)
            .HasMaxLength(20);

        builder.Property(c => c.AvatarUrl)
            .HasMaxLength(500);

        builder.HasOne(c => c.User)
            .WithOne(u => u.CustomerProfile)
            .HasForeignKey<CustomerProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
