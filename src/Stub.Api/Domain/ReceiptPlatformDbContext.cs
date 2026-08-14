using Microsoft.EntityFrameworkCore;

namespace Stub.Api.Domain;

public sealed class ReceiptPlatformDbContext(DbContextOptions<ReceiptPlatformDbContext> options) : DbContext(options)
{
    public DbSet<MerchantPortalConnectionEntity> Merchants => Set<MerchantPortalConnectionEntity>();
    public DbSet<DigitalReceiptEntity> Receipts => Set<DigitalReceiptEntity>();
    public DbSet<ReceiptLineItemEntity> ReceiptItems => Set<ReceiptLineItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantPortalConnectionEntity>(entity =>
        {
            entity.ToTable("merchants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.PosAccountId).IsRequired();
            entity.Property(x => x.PosProvider).IsRequired();
        });

        modelBuilder.Entity<DigitalReceiptEntity>(entity =>
        {
            entity.ToTable("receipts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransactionId).IsRequired();
            entity.Property(x => x.Currency).IsRequired();
            entity.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReceiptLineItemEntity>(entity =>
        {
            entity.ToTable("receipt_line_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
        });
    }
}

public sealed class MerchantPortalConnectionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PosAccountId { get; set; } = string.Empty;
    public string PosProvider { get; set; } = "square";
    public bool WebhookRegistered { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class DigitalReceiptEntity
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CustomerId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<ReceiptLineItemEntity> Items { get; set; } = [];
}

public sealed class ReceiptLineItemEntity
{
    public int Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
