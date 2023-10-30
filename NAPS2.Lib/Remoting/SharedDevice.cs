using NAPS2.Scan;

namespace NAPS2.Remoting;

public record SharedDevice
{
    public required string Name { get; init; }
    public required Driver Driver { get; init; }
    public required ScanDevice Device { get; init; }
}