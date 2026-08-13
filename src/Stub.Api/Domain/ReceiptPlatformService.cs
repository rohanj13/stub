using System.Collections.Concurrent;

namespace Stub.Api.Domain;

public sealed class ReceiptPlatformService
{
    private readonly ConcurrentDictionary<Guid, MerchantPortalConnection> _merchants = new();
    private readonly ConcurrentDictionary<Guid, DigitalReceipt> _receipts = new();

    public MerchantPortalConnection CreateMerchant(string name, string posAccountId)
    {
        var merchant = new MerchantPortalConnection
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            PosAccountId = posAccountId.Trim(),
            PosProvider = "square",
            WebhookRegistered = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _merchants[merchant.Id] = merchant;
        return merchant;
    }

    public IReadOnlyCollection<MerchantPortalConnection> GetMerchants() => _merchants.Values.OrderBy(m => m.Name).ToArray();

    public bool ConnectMerchantToSquare(Guid merchantId)
    {
        if (!_merchants.TryGetValue(merchantId, out var merchant))
        {
            return false;
        }

        _merchants[merchantId] = merchant with { WebhookRegistered = true };
        return true;
    }

    public DigitalReceipt ProcessSquareTransaction(SquareTransactionWebhook payload)
    {
        if (!_merchants.ContainsKey(payload.MerchantId))
        {
            throw new KeyNotFoundException("Merchant is not registered.");
        }

        var receipt = new DigitalReceipt
        {
            Id = Guid.NewGuid(),
            MerchantId = payload.MerchantId,
            TransactionId = payload.TransactionId.Trim(),
            OrderId = payload.OrderId?.Trim(),
            Total = payload.Total,
            Currency = payload.Currency.Trim().ToUpperInvariant(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Items = payload.Items.Select(item => new ReceiptLineItem
            {
                Name = item.Name.Trim(),
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToArray()
        };

        _receipts[receipt.Id] = receipt;
        return receipt;
    }

    public bool AssignCustomerToReceipt(Guid receiptId, string customerId)
    {
        if (!_receipts.TryGetValue(receiptId, out var receipt))
        {
            return false;
        }

        _receipts[receiptId] = receipt with { CustomerId = customerId.Trim() };
        return true;
    }

    public DigitalReceipt? GetReceipt(Guid receiptId) => _receipts.GetValueOrDefault(receiptId);

    public IReadOnlyCollection<DigitalReceipt> GetReceiptsByCustomer(string customerId)
    {
        return _receipts.Values
            .Where(r => string.Equals(r.CustomerId, customerId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToArray();
    }
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
