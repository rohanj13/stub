using Stub.Domain.Common;

namespace Stub.Domain.Entities;

public class ReceiptExtension : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public string Namespace { get; set; } = string.Empty;
    public string ExtensionType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty; // Will be jsonb in PostgreSQL
    
    // Navigation properties
    public Receipt Receipt { get; set; } = null!;
}
