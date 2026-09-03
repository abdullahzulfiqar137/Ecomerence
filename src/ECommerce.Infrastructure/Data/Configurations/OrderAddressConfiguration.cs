using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderAddressConfiguration : IEntityTypeConfiguration<OrderAddress>
{
    public void Configure(EntityTypeBuilder<OrderAddress> builder)
    {
        builder.ToTable("OrderAddresses");

        builder.HasKey(oa => oa.Id);

        builder.Property(oa => oa.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.Company)
            .HasMaxLength(150);

        builder.Property(oa => oa.StreetLine1)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(oa => oa.StreetLine2)
            .HasMaxLength(250);

        builder.Property(oa => oa.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.StateOrProvince)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.PostalCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(oa => oa.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(oa => oa.PhoneNumber)
            .HasMaxLength(30);
    }
}
