using Stub.Domain.Common;

namespace Stub.Domain.Entities;

public class ReceiptPayment : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ExternalPaymentId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    // Navigation properties
    public Receipt Receipt { get; set; } = null!;
}
