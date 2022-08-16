using NAPS2.Escl.Client;
using NAPS2.Escl.Server;
using Xunit;

namespace NAPS2.Escl.Tests;

public class ClientServerTests
{
    [Fact]
    public async Task ClientServer()
    {
        using var server = new EsclServer(new EsclServerConfig
        {
            Capabilities = new EsclCapabilities()
            {
                Version = "2.0",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc"
            }
        });
        server.Start();
        var client = new EsclHttpClient(new EsclService
        {
            Ip = "localhost",
            Port = 9898,
            RootUrl = "escl",
            Tls = false
        });
        var caps = await client.GetCapabilities();
        Assert.Equal("2.0", caps.Version);
        Assert.Equal("HP Blah", caps.MakeAndModel);
        Assert.Equal("123abc", caps.SerialNumber);
    }
}