using System.Net;
using NAPS2.Escl.Client;
using Xunit;

namespace NAPS2.Escl.Tests;

public class EsclClientTests
{
    [Theory]
    [InlineData("192.168.200.1", "192.168.100.50", null, "192.168.100.50")]
    [InlineData("192.168.200.1", "192.168.100.50", "2001:db8::50", "192.168.100.50")]
    [InlineData("2001:db8::1", "192.168.100.50", "2001:db8::50", "[2001:db8::50]")]
    [InlineData("192.168.200.1", null, "2001:db8::50", "[2001:db8::50]")]
    [InlineData("2001:db8::1", "192.168.100.50", null, "192.168.100.50")]
    [InlineData("fe80::50%7", null, "fe80::50", "[fe80::50%7]")]
    [InlineData("fe80::50%7", "192.168.100.50", "fe80::50", "[fe80::50%7]")]
    [InlineData("192.168.200.1", null, "fe80::50", "192.168.200.1")]
    [InlineData("192.168.200.1", null, null, "192.168.200.1")]
    [InlineData("192.168.100.50", "192.168.100.50", null, "192.168.100.50")]
    [InlineData("2001:db8::50", "192.168.100.50", "2001:db8::50", "[2001:db8::50]")]
    public void ServiceUrlsUseAdvertisedAddress(string remoteEndpoint, string? ipv4, string? ipv6, string expectedHost)
    {
        foreach (var tls in new[] { false, true })
        {
            var client = new EsclClient(new EsclService
            {
                RemoteEndpoint = IPAddress.Parse(remoteEndpoint),
                IpV4 = ipv4 == null ? null : IPAddress.Parse(ipv4),
                IpV6 = ipv6 == null ? null : IPAddress.Parse(ipv6),
                Host = "scanner.local",
                Port = 80,
                TlsPort = 443,
                Tls = tls,
                RootUrl = "eSCL",
                Uuid = "00000000-0000-0000-0000-000000000000",
                Thumbnail = "icon.png"
            });
            var host = expectedHost;
#if NET6_0_OR_GREATER
            if (OperatingSystem.IsMacOS())
            {
                host = "scanner.local";
            }
#endif
            var scheme = tls ? "https" : "http";
            var port = tls ? 443 : 80;
            Assert.Equal($"{scheme}://{host}:{port}/eSCL", client.ConnectionUri);
            Assert.Equal($"{scheme}://{host}:{port}/icon.png", client.IconUri);
        }
    }
}
