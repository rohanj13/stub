using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class Terminal : BaseEntity
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExternalTerminalId { get; set; } = string.Empty;
    public TerminalStatus Status { get; set; }
    
    // Navigation properties
    public Merchant Merchant { get; set; } = null!;
    public ICollection<HardwareDevice> HardwareDevices { get; set; } = new List<HardwareDevice>();
    public ICollection<RawTransaction> RawTransactions { get; set; } = new List<RawTransaction>();
    public ICollection<ReceiptAssignmentSession> ReceiptAssignmentSessions { get; set; } = new List<ReceiptAssignmentSession>();
}
