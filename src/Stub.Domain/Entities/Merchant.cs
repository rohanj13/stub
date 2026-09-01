using Stub.Domain.Common;

namespace Stub.Domain.Entities;

public class Merchant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string Abn { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Terminal> Terminals { get; set; } = new List<Terminal>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public ICollection<ProviderConnection> ProviderConnections { get; set; } = new List<ProviderConnection>();
}
