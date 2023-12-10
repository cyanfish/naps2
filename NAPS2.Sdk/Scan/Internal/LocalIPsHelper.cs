using System.Net.NetworkInformation;

namespace NAPS2.Scan.Internal;

internal static class LocalIPsHelper
{
    public static Task<HashSet<string>> Get()
    {
        return Task.Run(() =>
            NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address.ToString())
                .ToHashSet());
    }
}