using System.Security.Cryptography;
using System.Text;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public record SharedDevice
{
    public required string Name { get; init; }
    public required Driver Driver { get; init; }
    public required ScanDevice Device { get; init; }

    public string Uuid
    {
        get
        {
            var uniqueHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(Device.ID));
            return new Guid(uniqueHash.Take(16).ToArray()).ToString("D");
        }
    }
}