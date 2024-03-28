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
        var job = Substitute.For<IEsclScanJob>();
        using var server = new EsclServer();
        var uuid = Guid.NewGuid().ToString("D");
        var deviceConfig = new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.0",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc",
                Uuid = uuid
            },
            CreateJob = _ => job
        };
        server.AddDevice(deviceConfig);
        await server.Start();
        var client = new EsclClient(new EsclService
        {
            IpV4 = IPAddress.Loopback,
            IpV6 = IPAddress.IPv6Loopback,
            Host = $"[{IPAddress.IPv6Loopback}]",
            RemoteEndpoint = IPAddress.IPv6Loopback,
            Port = deviceConfig.Port,
            TlsPort = deviceConfig.TlsPort,
            RootUrl = "eSCL",
            Tls = false,
            Uuid = uuid
        });
        var caps = await client.GetCapabilities();
        Assert.Equal("2.0", caps.Version);
        Assert.Equal("HP Blah", caps.MakeAndModel);
        Assert.Equal("123abc", caps.SerialNumber);
    }

    [Fact]
    public async Task StartTlsServerWithoutCertificate()
    {
        using var server = new EsclServer();
        server.SecurityPolicy = EsclSecurityPolicy.RequireHttps;
        await Assert.ThrowsAsync<EsclSecurityPolicyViolationException>(() => server.Start());
    }
}