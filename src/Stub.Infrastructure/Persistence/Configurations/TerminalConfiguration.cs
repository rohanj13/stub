using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence.Configurations;

public class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("terminals");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.ExternalTerminalId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.HasIndex(t => t.ExternalTerminalId);

        builder.HasMany(t => t.HardwareDevices)
            .WithOne(hd => hd.Terminal)
            .HasForeignKey(hd => hd.TerminalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.RawTransactions)
            .WithOne(rt => rt.Terminal)
            .HasForeignKey(rt => rt.TerminalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.ReceiptAssignmentSessions)
            .WithOne(ras => ras.Terminal)
            .HasForeignKey(ras => ras.TerminalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
