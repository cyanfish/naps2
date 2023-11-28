using System.Net;
using NAPS2.Escl.Client;
using NAPS2.Escl.Server;
using NSubstitute;
using Xunit;

namespace NAPS2.Escl.Tests;

public class ClientServerTests
{
    [Fact]
    public async Task ClientServer()
    {
        // TODO: Any better way to prevent port collisions when running tests?
#if NET6_0_OR_GREATER
        int port = 9802;
#else
        int port = 9801;
#endif
        var job = Substitute.For<IEsclScanJob>();
        using var server = new EsclServer { Port = port };
        server.AddDevice(new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.0",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc"
            },
            CreateJob = _ => job
        });
        server.Start();
        var client = new EsclClient(new EsclService
        {
            IpV4 = IPAddress.Loopback,
            IpV6 = IPAddress.IPv6Loopback,
            Host = IPAddress.IPv6Loopback.ToString(),
            RemoteEndpoint = IPAddress.IPv6Loopback,
            Port = 9801,
            RootUrl = "eESCL",
            Tls = false
        });
        var caps = await client.GetCapabilities();
        Assert.Equal("2.0", caps.Version);
        Assert.Equal("HP Blah", caps.MakeAndModel);
        Assert.Equal("123abc", caps.SerialNumber);
    }
}