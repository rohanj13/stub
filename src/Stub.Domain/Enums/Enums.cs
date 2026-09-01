namespace Stub.Domain.Enums;

public enum TerminalStatus
{
    Active,
    Inactive,
    Suspended
}

public enum DeviceType
{
    NfcReader,
    NfcTag,
    QrDisplay,
    CustomerDisplay
}

public enum HardwareDeviceStatus
{
    Active,
    Inactive,
    Maintenance
}

public enum Provider
{
    Square,
    Shopify,
    Lightspeed,
    Other
}

public enum ProviderConnectionStatus
{
    Active,
    Inactive,
    Disconnected
}

public enum ReceiptIdentityStatus
{
    Active,
    Inactive,
    Suspended
}

public enum CredentialType
{
    Qr,
    Nfc,
    AppleWallet,
    GoogleWallet
}

public enum CredentialStatus
{
    Active,
    Inactive,
    Revoked
}

public enum ProcessingStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum ReceiptStatus
{
    Active,
    Void,
    Refunded
}

public enum AssignmentMethod
{
    Nfc,
    Qr,
    PaymentLinked,
    Email,
    Manual,
    Api
}

public enum AssignmentStatus
{
    Active,
    Revoked
}

public enum AssignmentSessionStatus
{
    Active,
    Completed,
    Expired,
    Cancelled
}
