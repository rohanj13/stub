using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class ReceiptCredential : BaseEntity
{
    public Guid ReceiptIdentityId { get; set; }
    public CredentialType CredentialType { get; set; }
    public string CredentialValue { get; set; } = string.Empty;
    public CredentialStatus Status { get; set; }
    
    // Navigation properties
    public ReceiptIdentity ReceiptIdentity { get; set; } = null!;
}
