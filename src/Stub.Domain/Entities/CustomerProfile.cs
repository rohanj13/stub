using Stub.Domain.Common;

namespace Stub.Domain.Entities;

public class CustomerProfile : BaseEntity
{
    public Guid ReceiptIdentityId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    // Navigation properties
    public ReceiptIdentity ReceiptIdentity { get; set; } = null!;
}
