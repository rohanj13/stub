using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptIdentityConfiguration : IEntityTypeConfiguration<ReceiptIdentity>
{
    public void Configure(EntityTypeBuilder<ReceiptIdentity> builder)
    {
        builder.ToTable("receipt_identities");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.PublicIdentifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ri => ri.Status)
            .IsRequired();

        builder.Property(ri => ri.CreatedAt)
            .IsRequired();

        builder.Property(ri => ri.UpdatedAt)
            .IsRequired();

        builder.HasIndex(ri => ri.PublicIdentifier)
            .IsUnique();

        builder.HasMany(ri => ri.ReceiptCredentials)
            .WithOne(rc => rc.ReceiptIdentity)
            .HasForeignKey(rc => rc.ReceiptIdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ri => ri.ReceiptAssignments)
            .WithOne(ra => ra.ReceiptIdentity)
            .HasForeignKey(ra => ra.ReceiptIdentityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ri => ri.CustomerProfile)
            .WithOne(cp => cp.ReceiptIdentity)
            .HasForeignKey<CustomerProfile>(cp => cp.ReceiptIdentityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
