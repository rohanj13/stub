using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class ReceiptAssignmentSession : BaseEntity
{
    public Guid TerminalId { get; set; }
    public Guid ReceiptIdentityId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public AssignmentSessionStatus Status { get; set; }
    
    // Navigation properties
    public Terminal Terminal { get; set; } = null!;
    public ReceiptIdentity ReceiptIdentity { get; set; } = null!;
}
