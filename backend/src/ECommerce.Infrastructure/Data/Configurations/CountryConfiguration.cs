using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.IsoCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PhonePrefix)
            .HasMaxLength(10);

        builder.Property(c => c.DefaultCurrency)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(c => c.IsoCode)
            .IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
