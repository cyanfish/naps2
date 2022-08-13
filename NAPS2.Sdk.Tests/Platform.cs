namespace NAPS2.Sdk.Tests;

[Flags]
public enum Platform
{
    None = 0,
    Windows = 1,
    Mac = 2,
    Linux = 4,
    X64 = 8,
    Arm64 = 16
}