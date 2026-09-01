using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("customer_profiles");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cp => cp.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cp => cp.CreatedAt)
            .IsRequired();

        builder.Property(cp => cp.UpdatedAt)
            .IsRequired();
    }
}
