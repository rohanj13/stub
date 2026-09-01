using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class ReceiptAssignment : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public Guid ReceiptIdentityId { get; set; }
    public AssignmentMethod AssignmentMethod { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public AssignmentStatus Status { get; set; }
    
    // Navigation properties
    public Receipt Receipt { get; set; } = null!;
    public ReceiptIdentity ReceiptIdentity { get; set; } = null!;
}
