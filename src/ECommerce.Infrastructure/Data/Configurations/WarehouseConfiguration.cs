using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.ContactPerson)
            .HasMaxLength(100);

        builder.Property(w => w.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(w => w.Email)
            .HasMaxLength(256);

        builder.Property(w => w.StreetLine1)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(w => w.StreetLine2)
            .HasMaxLength(250);

        builder.Property(w => w.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.StateOrProvince)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.PostalCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(w => w.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
