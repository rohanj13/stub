using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptExtensionConfiguration : IEntityTypeConfiguration<ReceiptExtension>
{
    public void Configure(EntityTypeBuilder<ReceiptExtension> builder)
    {
        builder.ToTable("receipt_extensions");

        builder.HasKey(re => re.Id);

        builder.Property(re => re.Namespace)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(re => re.ExtensionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(re => re.Data)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(re => re.CreatedAt)
            .IsRequired();

        builder.Property(re => re.UpdatedAt)
            .IsRequired();
    }
}
