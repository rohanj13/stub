using Stub.Domain.Common;

namespace Stub.Domain.Entities;

public class ReceiptItem : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Navigation properties
    public Receipt Receipt { get; set; } = null!;
}
