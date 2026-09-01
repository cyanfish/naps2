using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Escl;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Scan;

public class EsclMetadataCapsTests(ITestOutputHelper testOutputHelper) : ContextualTests(testOutputHelper),
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
    public async Task GetCaps_SetsProtocolVersionOnMetadataCaps()
    {
        var driver = new EsclScanDriver(ScanningContext);
        var device = new ScanDevice(Driver.Escl,
            $"http://127.0.0.1:{_deviceConfig.Port}/eSCL", "Test Scanner");

        var caps = await driver.GetCaps(new ScanOptions { Device = device }, CancellationToken.None);

        Assert.Equal("2.6", caps.MetadataCaps!.ProtocolVersion);
    }

    [Fact(Timeout = 60_000)]
    public async Task GetCaps_SetsProtocolVersionOnMetadataCaps_WhenVersionIsCustom()
    {
        // Use a second server with a distinct version to verify it is read back correctly
        using var server2 = new EsclServer();
        var customVersionConfig = new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.0",
                MakeAndModel = "Test Scanner",
                Uuid = Guid.NewGuid().ToString("D")
            },
            CreateJob = _ => Substitute.For<IEsclScanJob>()
        };
        server2.AddDevice(customVersionConfig);
        await server2.Start();

        var driver = new EsclScanDriver(ScanningContext);
        var device = new ScanDevice(Driver.Escl,
            $"http://127.0.0.1:{customVersionConfig.Port}/eSCL", "Test Scanner");

        var caps = await driver.GetCaps(new ScanOptions { Device = device }, CancellationToken.None);

        Assert.Equal("2.0", caps.MetadataCaps!.ProtocolVersion);
    }

    [Fact(Timeout = 60_000)]
    public async Task GetCaps_SetsDefaultProtocolVersionOnMetadataCaps_WhenVersionNotExplicitlySet()
    {
        using var server2 = new EsclServer();
        var defaultVersionConfig = new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                // Version not set — EsclCapabilities.Version defaults to EsclCapabilities.DEFAULT_VERSION
                MakeAndModel = "Test Scanner",
                Uuid = Guid.NewGuid().ToString("D")
            },
            CreateJob = _ => Substitute.For<IEsclScanJob>()
        };
        server2.AddDevice(defaultVersionConfig);
        await server2.Start();

        var driver = new EsclScanDriver(ScanningContext);
        var device = new ScanDevice(Driver.Escl,
            $"http://127.0.0.1:{defaultVersionConfig.Port}/eSCL", "Test Scanner");

        var caps = await driver.GetCaps(new ScanOptions { Device = device }, CancellationToken.None);

        Assert.Equal(EsclCapabilities.DEFAULT_VERSION, caps.MetadataCaps!.ProtocolVersion);
    }
}
