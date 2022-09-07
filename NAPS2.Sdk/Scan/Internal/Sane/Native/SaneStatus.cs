namespace NAPS2.Scan.Internal.Sane.Native;

public enum SaneStatus
{
    Good = 0,
    Unsupported = 1,
    Cancelled = 2,
    DeviceBusy = 3,
    Invalid = 4,
    Eof = 5,
    Jammed = 6,
    NoDocs = 7,
    CoverOpen = 8,
    IoError = 9,
    NoMem = 10,
    AccessDenied = 11
}