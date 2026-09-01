using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class RawTransaction : BaseEntity
{
    public Guid MerchantId { get; set; }
    public Guid? TerminalId { get; set; }
    public Guid? ProviderConnectionId { get; set; }
    public Provider Provider { get; set; }
    public string ProviderEventType { get; set; } = string.Empty;
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty; // Will be jsonb in PostgreSQL
    public DateTimeOffset ReceivedAt { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public string? ProcessingError { get; set; }
    
    // Navigation properties
    public Merchant Merchant { get; set; } = null!;
    public Terminal? Terminal { get; set; }
    public ProviderConnection? ProviderConnection { get; set; }
    public Receipt? Receipt { get; set; }
}
