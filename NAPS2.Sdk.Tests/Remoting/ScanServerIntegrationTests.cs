using Microsoft.Extensions.Logging;
using NAPS2.Escl.Server;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Mocks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class ScanServerIntegrationTests : ContextualTests
{
    private readonly ScanServer _server;
    private readonly MockScanBridge _bridge;
    private readonly ScanController _client;
    private readonly ScanDevice _clientDevice;

    public ScanServerIntegrationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _server = new ScanServer(ScanningContext, new EsclServer());

        // Set up a server connecting to a mock scan backend
        _bridge = new MockScanBridge
        {
            MockOutput = [CreateScannedImage()]
        };
        var scanBridgeFactory = Substitute.For<IScanBridgeFactory>();
        scanBridgeFactory.Create(Arg.Any<ScanOptions>()).Returns(_bridge);
        _server.ScanController = new ScanController(ScanningContext, scanBridgeFactory);

        // Initialize the server with a single device with a unique ID for the test
        var displayName = $"testName{Guid.NewGuid()}";
        ScanningContext.Logger.LogDebug("Display name: {Name}", displayName);
        var serverDevice = new ScanDevice(ScanOptionsValidator.SystemDefaultDriver, "testID", "testName");
        var serverSharedDevice = new SharedDevice { Device = serverDevice, Name = displayName };
        _server.RegisterDevice(serverSharedDevice);
        _server.Start().Wait();

        // Set up a client ScanController for scanning through EsclScanDriver -> network -> ScanServer
        _client = new ScanController(ScanningContext);
        // This device won't match exactly the real device from GetDeviceList but it includes the UUID which is enough
        // for EsclScanDriver to correctly identify the server for scanning.
        _clientDevice = new ScanDevice(Driver.Escl, $"|{serverSharedDevice.GetUuid(_server.InstanceId)}", displayName);
    }

    public override void Dispose()
    {
        _server.Dispose();
        base.Dispose();
    }

    [Fact]
    public async Task FindDevice()
    {
        var devices = await _client.GetDeviceList(Driver.Escl);
        // The device name is suffixed with the IP so we just check the prefix matches (and vice versa for ID)
        Assert.Contains(devices,
            device => device.Name.StartsWith(_clientDevice.Name) && device.ID.EndsWith(_clientDevice.ID));
    }

    [Fact]
    public async Task Scan()
    {
        var images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice
        }).ToListAsync();
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public async Task ScanMultiplePages()
    {
        _bridge.MockOutput =
            CreateScannedImages(ImageResources.dog, ImageResources.dog_h_n300, ImageResources.dog_h_p300).ToList();
        var images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            PaperSource = PaperSource.Feeder
        }).ToListAsync();
        Assert.Equal(3, images.Count);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
        ImageAsserts.Similar(ImageResources.dog_h_n300, images[1]);
        ImageAsserts.Similar(ImageResources.dog_h_p300, images[2]);
    }

    [Fact]
    public async Task ScanWithCorrectOptions()
    {
        var images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            BitDepth = BitDepth.Color,
            Dpi = 100,
            PaperSource = PaperSource.Flatbed,
            PageSize = PageSize.Letter,
            PageAlign = HorizontalAlign.Right
        }).ToListAsync();

        var opts = _bridge.LastOptions;
        Assert.Equal(BitDepth.Color, opts.BitDepth);
        Assert.Equal(100, opts.Dpi);
        Assert.Equal(PaperSource.Flatbed, opts.PaperSource);
        Assert.Equal(PageSize.Letter, opts.PageSize);
        Assert.Equal(HorizontalAlign.Right, opts.PageAlign);
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);

        _bridge.MockOutput = CreateScannedImages(ImageResources.dog_gray).ToList();
        images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            BitDepth = BitDepth.Grayscale,
            Dpi = 300,
            PaperSource = PaperSource.Feeder,
            PageSize = PageSize.Legal,
            PageAlign = HorizontalAlign.Center
        }).ToListAsync();

        opts = _bridge.LastOptions;
        Assert.Equal(BitDepth.Grayscale, opts.BitDepth);
        Assert.Equal(300, opts.Dpi);
        Assert.Equal(PaperSource.Feeder, opts.PaperSource);
        Assert.Equal(PageSize.Legal, opts.PageSize);
        Assert.Equal(HorizontalAlign.Center, opts.PageAlign);
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog_gray, images[0]);

        _bridge.MockOutput = CreateScannedImages(ImageResources.dog_bw).ToList();
        images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            BitDepth = BitDepth.BlackAndWhite,
            Dpi = 4800,
            PaperSource = PaperSource.Duplex,
            PageSize = PageSize.A3,
            PageAlign = HorizontalAlign.Left
        }).ToListAsync();

        opts = _bridge.LastOptions;
        Assert.Equal(BitDepth.BlackAndWhite, opts.BitDepth);
        Assert.Equal(4800, opts.Dpi);
        Assert.Equal(PaperSource.Duplex, opts.PaperSource);
        Assert.Equal(PageSize.A3.WidthInMm, opts.PageSize!.WidthInMm, 1);
        Assert.Equal(PageSize.A3.HeightInMm, opts.PageSize!.HeightInMm, 1);
        Assert.Equal(HorizontalAlign.Left, opts.PageAlign);
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog_bw, images[0]);
    }

    [Fact]
    public async Task ScanWithError()
    {
        _bridge.Error = new NoPagesException();

        await Assert.ThrowsAsync<NoPagesException>(async () => await _client.Scan(new ScanOptions
        {
            Device = _clientDevice
        }).ToListAsync());
    }
}