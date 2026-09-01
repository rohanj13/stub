using Stub.Domain.Common;
using Stub.Domain.Enums;

namespace Stub.Domain.Entities;

public class HardwareDevice : BaseEntity
{
    public Guid TerminalId { get; set; }
    public DeviceType DeviceType { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public HardwareDeviceStatus Status { get; set; }
    
    // Navigation properties
    public Terminal Terminal { get; set; } = null!;
}
