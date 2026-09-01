using Microsoft.EntityFrameworkCore;
using Stub.Domain.Entities;

namespace Stub.Infrastructure.Persistence;

public class StubDbContext : DbContext
{
    public StubDbContext(DbContextOptions<StubDbContext> options) : base(options)
    {
    }

    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<HardwareDevice> HardwareDevices => Set<HardwareDevice>();
    public DbSet<ProviderConnection> ProviderConnections => Set<ProviderConnection>();
    public DbSet<ReceiptIdentity> ReceiptIdentities => Set<ReceiptIdentity>();
    public DbSet<ReceiptCredential> ReceiptCredentials => Set<ReceiptCredential>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<RawTransaction> RawTransactions => Set<RawTransaction>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptItem> ReceiptItems => Set<ReceiptItem>();
    public DbSet<ReceiptPayment> ReceiptPayments => Set<ReceiptPayment>();
    public DbSet<ReceiptExtension> ReceiptExtensions => Set<ReceiptExtension>();
    public DbSet<ReceiptAssignment> ReceiptAssignments => Set<ReceiptAssignment>();
    public DbSet<ReceiptAssignmentSession> ReceiptAssignmentSessions => Set<ReceiptAssignmentSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StubDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTimeOffset.UtcNow;
            }

            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
