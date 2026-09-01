using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class RawTransactionConfiguration : IEntityTypeConfiguration<RawTransaction>
{
    public void Configure(EntityTypeBuilder<RawTransaction> builder)
    {
        builder.ToTable("raw_transactions");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Provider)
            .IsRequired();

        builder.Property(rt => rt.ProviderEventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.ExternalTransactionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.Payload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(rt => rt.ReceivedAt)
            .IsRequired();

        builder.Property(rt => rt.ProcessingStatus)
            .IsRequired();

        builder.Property(rt => rt.ProcessingError)
            .HasMaxLength(1000);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired();

        builder.HasIndex(rt => rt.ExternalTransactionId);

        builder.HasOne(rt => rt.Receipt)
            .WithOne(r => r.RawTransaction)
            .HasForeignKey<Receipt>(r => r.RawTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
