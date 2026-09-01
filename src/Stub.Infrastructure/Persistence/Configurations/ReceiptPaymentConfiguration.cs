using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptPaymentConfiguration : IEntityTypeConfiguration<ReceiptPayment>
{
    public void Configure(EntityTypeBuilder<ReceiptPayment> builder)
    {
        builder.ToTable("receipt_payments");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rp => rp.ExternalPaymentId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rp => rp.PaymentMethod)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rp => rp.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(rp => rp.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(rp => rp.CreatedAt)
            .IsRequired();

        builder.Property(rp => rp.UpdatedAt)
            .IsRequired();
    }
}
