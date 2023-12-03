using System.Diagnostics;
using NAPS2.Escl.Server;
using NSubstitute;
using Xunit;

namespace NAPS2.Escl.Tests;

public class AdvertiseTests
{
    [Fact]
    public async Task Advertise()
    {
        var job = Substitute.For<IEsclScanJob>();
        using var server = new EsclServer();
        server.AddDevice(new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.6",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc",
                Uuid = Guid.NewGuid().ToString("D")
            },
            CreateJob = _ => job
        });
        await server.Start();
        if (Debugger.IsAttached)
        {
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(5000);
            }
        }
    }
}