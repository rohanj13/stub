using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.ExternalTransactionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.IssuedAt)
            .IsRequired();

        builder.Property(r => r.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.TaxTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.DiscountTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        builder.HasIndex(r => r.ExternalTransactionId);

        builder.HasMany(r => r.ReceiptItems)
            .WithOne(ri => ri.Receipt)
            .HasForeignKey(ri => ri.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.ReceiptPayments)
            .WithOne(rp => rp.Receipt)
            .HasForeignKey(rp => rp.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.ReceiptExtensions)
            .WithOne(re => re.Receipt)
            .HasForeignKey(re => re.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.ReceiptAssignments)
            .WithOne(ra => ra.Receipt)
            .HasForeignKey(ra => ra.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
