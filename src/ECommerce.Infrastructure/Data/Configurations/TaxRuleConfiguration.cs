using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("TaxRules");

        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(tr => tr.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(tr => tr.StateOrProvince)
            .HasMaxLength(100);

        builder.Property(tr => tr.PostalCode)
            .HasMaxLength(30);

        builder.Property(tr => tr.Rate)
            .HasPrecision(18, 4);

        builder.HasIndex(tr => new { tr.CountryCode, tr.StateOrProvince });
        builder.HasQueryFilter(tr => !tr.IsDeleted);
    }
}
