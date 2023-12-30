using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public record SharedDevice
{
    public required string Name { get; init; }
    public required ScanDevice Device { get; init; }
    public int Port { get; init; }

    public virtual bool Equals(SharedDevice? other) =>
        other is not null && Name == other.Name && Device == other.Device;

    public override int GetHashCode() =>
        Name.GetHashCode() * 23 + Device.GetHashCode();
}