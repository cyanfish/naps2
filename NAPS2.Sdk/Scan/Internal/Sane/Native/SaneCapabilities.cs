namespace NAPS2.Scan.Internal.Sane.Native;

[Flags]
public enum SaneCapabilities
{
    None = 0,
    SoftSelect = 1,
    HardSelect = 2,
    SoftDetect = 4,
    Emulated = 8,
    Automatic = 16,
    Inactive = 32,
    Advanced = 64
}