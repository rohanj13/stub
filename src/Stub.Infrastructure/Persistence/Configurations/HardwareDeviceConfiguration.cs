using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class HardwareDeviceConfiguration : IEntityTypeConfiguration<HardwareDevice>
{
    public void Configure(EntityTypeBuilder<HardwareDevice> builder)
    {
        builder.ToTable("hardware_devices");

        builder.HasKey(hd => hd.Id);

        builder.Property(hd => hd.DeviceType)
            .IsRequired();

        builder.Property(hd => hd.Identifier)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(hd => hd.Status)
            .IsRequired();

        builder.Property(hd => hd.CreatedAt)
            .IsRequired();

        builder.Property(hd => hd.UpdatedAt)
            .IsRequired();
    }
}
