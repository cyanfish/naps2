using System.Security.Cryptography;
using System.Text;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

internal record ScanServerDevice
{
    public required string Name { get; init; }
    public required ScanDevice Device { get; init; }
    public int Port { get; init; }

    public string GetUuid(Guid instanceId)
    {
        var key = $"{Device.Driver};{Device.ID};{Name};{instanceId}";
        var uniqueHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
        return new Guid(uniqueHash.Take(16).ToArray()).ToString("D");
    }

    public virtual bool Equals(ScanServerDevice? other) =>
        other is not null && Name == other.Name && Device == other.Device;

    public override int GetHashCode() =>
        Name.GetHashCode() * 23 + Device.GetHashCode();
}