using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class ReceiptAssignmentConfiguration : IEntityTypeConfiguration<ReceiptAssignment>
{
    public void Configure(EntityTypeBuilder<ReceiptAssignment> builder)
    {
        builder.ToTable("receipt_assignments");

        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.AssignmentMethod)
            .IsRequired();

        builder.Property(ra => ra.AssignedAt)
            .IsRequired();

        builder.Property(ra => ra.Status)
            .IsRequired();

        builder.Property(ra => ra.CreatedAt)
            .IsRequired();

        builder.Property(ra => ra.UpdatedAt)
            .IsRequired();
    }
}
