using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class ProviderConnection : BaseEntity
{
    public Guid MerchantId { get; set; }
    public Provider Provider { get; set; }
    public string ExternalMerchantId { get; set; } = string.Empty;
    public string ExternalLocationId { get; set; } = string.Empty;
    public ProviderConnectionStatus Status { get; set; }
    
    // Navigation properties
    public Merchant Merchant { get; set; } = null!;
}
