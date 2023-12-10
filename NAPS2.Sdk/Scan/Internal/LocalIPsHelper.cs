using System.Net;
using System.Net.NetworkInformation;

namespace NAPS2.Scan.Internal;

internal static class LocalIPsHelper
{
    public static Task<HashSet<IPAddress>> Get()
    {
        return Task.Run(() =>
            NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address)
                .ToHashSet());
    }
}