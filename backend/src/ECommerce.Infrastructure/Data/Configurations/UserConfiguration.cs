using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.NormalizedUserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(30);

        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(u => u.NormalizedUserName)
            .IsUnique();

        builder.HasIndex(u => u.IsDeleted);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
