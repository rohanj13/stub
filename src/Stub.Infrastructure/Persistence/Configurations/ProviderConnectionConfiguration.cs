using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ProviderConnectionConfiguration : IEntityTypeConfiguration<ProviderConnection>
{
    public void Configure(EntityTypeBuilder<ProviderConnection> builder)
    {
        builder.ToTable("provider_connections");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Provider)
            .IsRequired();

        builder.Property(pc => pc.ExternalMerchantId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pc => pc.ExternalLocationId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pc => pc.Status)
            .IsRequired();

        builder.Property(pc => pc.CreatedAt)
            .IsRequired();

        builder.Property(pc => pc.UpdatedAt)
            .IsRequired();
    }
}
