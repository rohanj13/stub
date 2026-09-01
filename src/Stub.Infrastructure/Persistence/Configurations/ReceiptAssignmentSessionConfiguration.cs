using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptAssignmentSessionConfiguration : IEntityTypeConfiguration<ReceiptAssignmentSession>
{
    public void Configure(EntityTypeBuilder<ReceiptAssignmentSession> builder)
    {
        builder.ToTable("receipt_assignment_sessions");

        builder.HasKey(ras => ras.Id);

        builder.Property(ras => ras.StartedAt)
            .IsRequired();

        builder.Property(ras => ras.ExpiresAt)
            .IsRequired();

        builder.Property(ras => ras.Status)
            .IsRequired();

        builder.Property(ras => ras.CreatedAt)
            .IsRequired();

        builder.Property(ras => ras.UpdatedAt)
            .IsRequired();
    }
}
