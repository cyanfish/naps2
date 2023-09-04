using System.Diagnostics;
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
            Capabilities = new EsclCapabilities
            {
                Version = "2.6",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc"
            }
        });
        server.Start();
        if (Debugger.IsAttached)
        {
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(5000);
            }
        }
    }
}