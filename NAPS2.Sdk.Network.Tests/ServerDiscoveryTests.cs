using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Remoting.Network.Tests;

// TODO: Try and reduce flakiness without needing to bump up timeouts
public class ServerDiscoveryTests
{
    // TODO: Deflake
    [Fact(Skip = "flaky")]
    public async Task ServerDiscovery()
    {
        using var server = CreateServer(new NetworkScanServerOptions
        {
            ServerName = "NetworkScanTests.ServerDiscovery"
        });
        var client = new NetworkScanClient();
        await server.Start();
        await Task.Delay(500);
        var discovered = await client.DiscoverServers(500);
        Assert.Contains(discovered, x => x.Name == "NetworkScanTests.ServerDiscovery");
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public async Task ServerDiscoveryCustomPort()
    {
        using var server = CreateServer(new NetworkScanServerOptions
        {
            ServerName = "NetworkScanTests.ServerDiscoveryCustomPort",
            DiscoveryPort = 33433
        });
        var client = new NetworkScanClient(new NetworkScanClientOptions
        {
            DiscoveryPort = 33433
        });
        await server.Start();
        await Task.Delay(500);
        var discovered = await client.DiscoverServers(500);
        Assert.Contains(discovered, x => x.Name == "NetworkScanTests.ServerDiscoveryCustomPort");
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public async Task ServerDiscoveryMismatchPort()
    {
        using var server = CreateServer(new NetworkScanServerOptions
        {
            ServerName = "NetworkScanTests.ServerDiscoveryMismatchPort",
            DiscoveryPort = 33444
        });
        var client = new NetworkScanClient(new NetworkScanClientOptions
        {
            DiscoveryPort = 33555
        });
        await server.Start();
        await Task.Delay(500);
        var discovered = await client.DiscoverServers(500);
        Assert.DoesNotContain(discovered, x => x.Name == "NetworkScanTests.ServerDiscoveryMismatchPort");
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public async Task ServerDiscoveryOff()
    {
        using var server = CreateServer(new NetworkScanServerOptions
        {
            ServerName = "NetworkScanTests.ServerDiscoveryOff",
            AllowDiscovery = false
        });
        var client = new NetworkScanClient();
        await server.Start();
        await Task.Delay(500);
        var discovered = await client.DiscoverServers(500);
        Assert.DoesNotContain(discovered, x => x.Name == "NetworkScanTests.ServerDiscoveryOff");
    }

    private NetworkScanServer CreateServer(NetworkScanServerOptions options)
    {
        var scanningContext = new ScanningContext(TestImageContextFactory.Get());
        return new NetworkScanServer(scanningContext, options);
    }
}