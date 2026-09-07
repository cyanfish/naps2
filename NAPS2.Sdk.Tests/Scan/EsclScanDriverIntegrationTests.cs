using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Escl;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Scan;

public class EsclScanDriverIntegrationTests(ITestOutputHelper testOutputHelper) : ContextualTests(testOutputHelper),
    IAsyncLifetime
{
    private EsclServer _server = null!;
    private EsclDeviceConfig _deviceConfig = null!;

    public async Task InitializeAsync()
    {
        _server = new EsclServer();
        _deviceConfig = new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.6",
                MakeAndModel = "Test Scanner",
                Manufacturer = "Test Manufacturer",
                SerialNumber = "SN-12345",
                Uuid = Guid.NewGuid().ToString("D")
            },
            CreateJob = _ => Substitute.For<IEsclScanJob>()
        };
        _server.AddDevice(_deviceConfig);
        await _server.Start();
    }

    public Task DisposeAsync()
    {
        _server.Dispose();
        return Task.CompletedTask;
    }

    [Fact(Timeout = 60_000)]
    public async Task GetCaps_ReturnsCapsFromRealServer()
    {
        var driver = new EsclScanDriver(ScanningContext);
        var device = new ScanDevice(Driver.Escl,
            $"http://127.0.0.1:{_deviceConfig.Port}/eSCL", "Test Scanner");

        var caps = await driver.GetCaps(new ScanOptions { Device = device }, CancellationToken.None);

        Assert.Equal("Test Scanner", caps.MetadataCaps!.Model);
        Assert.Equal("Test Manufacturer", caps.MetadataCaps!.Manufacturer);
        Assert.Equal("SN-12345", caps.MetadataCaps!.SerialNumber);
        Assert.Equal("2.6", caps.MetadataCaps!.ProtocolVersion);
    }
}
