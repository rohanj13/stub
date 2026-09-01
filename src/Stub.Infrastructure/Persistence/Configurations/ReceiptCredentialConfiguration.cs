using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptCredentialConfiguration : IEntityTypeConfiguration<ReceiptCredential>
{
    public void Configure(EntityTypeBuilder<ReceiptCredential> builder)
    {
        builder.ToTable("receipt_credentials");

        builder.HasKey(rc => rc.Id);

        builder.Property(rc => rc.CredentialType)
            .IsRequired();

        builder.Property(rc => rc.CredentialValue)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rc => rc.Status)
            .IsRequired();

        builder.Property(rc => rc.CreatedAt)
            .IsRequired();

        builder.Property(rc => rc.UpdatedAt)
            .IsRequired();

        builder.HasIndex(rc => rc.CredentialValue);
    }
}
