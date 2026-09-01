using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class Receipt : BaseEntity
{
    public Guid MerchantId { get; set; }
    public Guid RawTransactionId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string ExternalTransactionId { get; set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ReceiptStatus Status { get; set; }
    
    // Navigation properties
    public Merchant Merchant { get; set; } = null!;
    public RawTransaction RawTransaction { get; set; } = null!;
    public ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
    public ICollection<ReceiptPayment> ReceiptPayments { get; set; } = new List<ReceiptPayment>();
    public ICollection<ReceiptExtension> ReceiptExtensions { get; set; } = new List<ReceiptExtension>();
    public ICollection<ReceiptAssignment> ReceiptAssignments { get; set; } = new List<ReceiptAssignment>();
}
