using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class ReceiptIdentity : BaseEntity
{
    public string PublicIdentifier { get; set; } = string.Empty;
    public ReceiptIdentityStatus Status { get; set; }
    
    // Navigation properties
    public ICollection<ReceiptCredential> ReceiptCredentials { get; set; } = new List<ReceiptCredential>();
    public ICollection<ReceiptAssignment> ReceiptAssignments { get; set; } = new List<ReceiptAssignment>();
    public CustomerProfile? CustomerProfile { get; set; }
}
