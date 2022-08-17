using NAPS2.Escl.Server;
using Xunit;

namespace NAPS2.Escl.Tests;

public class AdvertiseTests
{
    [Fact]
    public async Task Advertise()
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
        using var advertiser = new MdnsAdvertiser();
        advertiser.Advertise();
        await Task.Delay(10000);
    }
}