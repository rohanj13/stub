using Microsoft.EntityFrameworkCore;

namespace Stub.Api.Domain;

public sealed class ReceiptPlatformService
{
    private readonly ReceiptPlatformDbContext _dbContext;

    public ReceiptPlatformService(ReceiptPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public MerchantPortalConnection CreateMerchant(string name, string posAccountId)
    {
        var merchant = new MerchantPortalConnectionEntity
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            PosAccountId = posAccountId.Trim(),
            PosProvider = "square",
            WebhookRegistered = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _dbContext.Merchants.Add(merchant);
        _dbContext.SaveChanges();
        return MapMerchant(merchant);
    }

    public IReadOnlyCollection<MerchantPortalConnection> GetMerchants() => _dbContext.Merchants
        .AsNoTracking()
        .OrderBy(m => m.Name)
        .Select(MapMerchant)
        .ToArray();

    public bool ConnectMerchantToSquare(Guid merchantId)
    {
        var merchant = _dbContext.Merchants.Find(merchantId);
        if (merchant is null)
        {
            return false;
        }

        merchant.WebhookRegistered = true;
        _dbContext.SaveChanges();
        return true;
    }

    public DigitalReceipt ProcessSquareTransaction(SquareTransactionWebhook payload)
    {
        if (!_dbContext.Merchants.Any(m => m.Id == payload.MerchantId))
        {
            throw new KeyNotFoundException("Merchant is not registered.");
        }

        var receipt = new DigitalReceiptEntity
        {
            Id = Guid.NewGuid(),
            MerchantId = payload.MerchantId,
            TransactionId = payload.TransactionId.Trim(),
            OrderId = payload.OrderId?.Trim(),
            Total = payload.Total,
            Currency = payload.Currency.Trim().ToUpperInvariant(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Items = payload.Items.Select(item => new ReceiptLineItemEntity
            {
                Name = item.Name.Trim(),
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToArray()
        };

        _dbContext.Receipts.Add(receipt);
        _dbContext.SaveChanges();
        return MapReceipt(receipt);
    }

    public bool AssignCustomerToReceipt(Guid receiptId, string customerId)
    {
        var receipt = _dbContext.Receipts.Find(receiptId);
        if (receipt is null)
        {
            return false;
        }

        receipt.CustomerId = customerId.Trim();
        _dbContext.SaveChanges();
        return true;
    }

    public DigitalReceipt? GetReceipt(Guid receiptId)
    {
        var receipt = _dbContext.Receipts
            .AsNoTracking()
            .Include(r => r.Items)
            .FirstOrDefault(r => r.Id == receiptId);
        return receipt is null ? null : MapReceipt(receipt);
    }

    public IReadOnlyCollection<DigitalReceipt> GetReceiptsByCustomer(string customerId)
    {
        var normalizedCustomerId = customerId.Trim();
        return _dbContext.Receipts
            .AsNoTracking()
            .Include(r => r.Items)
            .Where(r => r.CustomerId != null && EF.Functions.ILike(r.CustomerId, normalizedCustomerId))
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(MapReceipt)
            .ToArray();
    }

    private static MerchantPortalConnection MapMerchant(MerchantPortalConnectionEntity merchant) => new()
    {
        Id = merchant.Id,
        Name = merchant.Name,
        PosAccountId = merchant.PosAccountId,
        PosProvider = merchant.PosProvider,
        WebhookRegistered = merchant.WebhookRegistered,
        CreatedAtUtc = merchant.CreatedAtUtc
    };

    private static DigitalReceipt MapReceipt(DigitalReceiptEntity receipt) => new()
    {
        Id = receipt.Id,
        MerchantId = receipt.MerchantId,
        TransactionId = receipt.TransactionId,
        OrderId = receipt.OrderId,
        Total = receipt.Total,
        Currency = receipt.Currency,
        CustomerId = receipt.CustomerId,
        CreatedAtUtc = receipt.CreatedAtUtc,
        Items = receipt.Items
            .Select(item => new ReceiptLineItem
            {
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            })
            .ToArray()
    };
}

public sealed record MerchantPortalConnection
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PosAccountId { get; init; } = string.Empty;
    public string PosProvider { get; init; } = "square";
    public bool WebhookRegistered { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record ReceiptLineItem
{
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
}

public sealed record DigitalReceipt
{
    public Guid Id { get; init; }
    public Guid MerchantId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public string? OrderId { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "USD";
    public string? CustomerId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public IReadOnlyList<ReceiptLineItem> Items { get; init; } = [];
}

public sealed record CreateMerchantRequest(string Name, string PosAccountId);

public sealed record AssignCustomerRequest(string CustomerId);

public sealed record SquareTransactionWebhook(
    Guid MerchantId,
    string TransactionId,
    string? OrderId,
    decimal Total,
    string Currency,
    IReadOnlyList<SquareTransactionItem> Items);

public sealed record SquareTransactionItem(string Name, decimal UnitPrice, int Quantity);
