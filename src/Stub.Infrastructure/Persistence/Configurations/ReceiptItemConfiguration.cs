using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItem>
{
    public void Configure(EntityTypeBuilder<ReceiptItem> builder)
    {
        builder.ToTable("receipt_items");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ri => ri.Description)
            .HasMaxLength(500);

        builder.Property(ri => ri.Sku)
            .HasMaxLength(100);

        builder.Property(ri => ri.Quantity)
            .IsRequired();

        builder.Property(ri => ri.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ri => ri.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ri => ri.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ri => ri.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ri => ri.CreatedAt)
            .IsRequired();

        builder.Property(ri => ri.UpdatedAt)
            .IsRequired();
    }
}
