using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("merchants");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.LegalName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Abn)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        builder.HasMany(m => m.Terminals)
            .WithOne(t => t.Merchant)
            .HasForeignKey(t => t.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Receipts)
            .WithOne(r => r.Merchant)
            .HasForeignKey(r => r.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.ProviderConnections)
            .WithOne(pc => pc.Merchant)
            .HasForeignKey(pc => pc.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
