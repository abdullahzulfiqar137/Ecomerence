using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderNoteConfiguration : IEntityTypeConfiguration<OrderNote>
{
    public void Configure(EntityTypeBuilder<OrderNote> builder)
    {
        builder.ToTable("OrderNotes");

        builder.HasKey(on => on.Id);

        builder.Property(on => on.Note)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(on => on.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(on => on.Order)
            .WithMany(o => o.OrderNotes)
            .HasForeignKey(on => on.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(on => on.OrderId);
    }
}
