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
        _bridge = new MockScanBridge();
        var scanBridgeFactory = Substitute.For<IScanBridgeFactory>();
        scanBridgeFactory.Create(Arg.Any<ScanOptions>()).Returns(_bridge);
        _server.ScanController = new ScanController(ScanningContext, scanBridgeFactory);

        // Initialize the server with a single device with a unique ID for the test
        var displayName = $"testName-{Guid.NewGuid()}";
        ScanningContext.Logger.LogDebug("Display name: {Name}", displayName);
        var serverDevice = new ScanDevice(ScanOptionsValidator.SystemDefaultDriver, "testID", "testName");
        _server.RegisterDevice(serverDevice, displayName);
        _server.Start().Wait();

        // Set up a client ScanController for scanning through EsclScanDriver -> network -> ScanServer
        _client = new ScanController(ScanningContext);
        // This device won't match exactly the real device from GetDeviceList but it includes the UUID which is enough
        // for EsclScanDriver to correctly identify the server for scanning.
        var uuid = new ScanServerDevice { Device = serverDevice, Name = displayName }.GetUuid(_server.InstanceId);
        _clientDevice = new ScanDevice(Driver.Escl, $"|{uuid}", displayName);
    }

    public override void Dispose()
    {
        _server.Stop().Wait();
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
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
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
            CreateScannedImages(ImageResources.dog, ImageResources.dog_h_n300, ImageResources.dog_h_p300);
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
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
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
        Assert.Equal(75, opts.Quality);
        Assert.False(opts.MaxQuality);
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);

        _bridge.MockOutput = CreateScannedImages(ImageResources.dog_gray);
        images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            BitDepth = BitDepth.Grayscale,
            Dpi = 300,
            PaperSource = PaperSource.Feeder,
            PageSize = PageSize.Legal,
            PageAlign = HorizontalAlign.Center,
            Quality = 0,
            MaxQuality = true
        }).ToListAsync();

        opts = _bridge.LastOptions;
        Assert.Equal(BitDepth.Grayscale, opts.BitDepth);
        Assert.Equal(300, opts.Dpi);
        Assert.Equal(PaperSource.Feeder, opts.PaperSource);
        Assert.Equal(PageSize.Legal, opts.PageSize);
        Assert.Equal(HorizontalAlign.Center, opts.PageAlign);
        Assert.Equal(0, opts.Quality);
        Assert.True(opts.MaxQuality);
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog_gray, images[0]);

        _bridge.MockOutput = CreateScannedImages(ImageResources.dog_bw);
        images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            BitDepth = BitDepth.BlackAndWhite,
            Dpi = 4800,
            PaperSource = PaperSource.Duplex,
            PageSize = PageSize.A3,
            PageAlign = HorizontalAlign.Left,
            Quality = 100
        }).ToListAsync();

        opts = _bridge.LastOptions;
        Assert.Equal(BitDepth.BlackAndWhite, opts.BitDepth);
        Assert.Equal(4800, opts.Dpi);
        Assert.Equal(PaperSource.Duplex, opts.PaperSource);
        Assert.Equal(PageSize.A3.WidthInMm, opts.PageSize!.WidthInMm, 1);
        Assert.Equal(PageSize.A3.HeightInMm, opts.PageSize!.HeightInMm, 1);
        Assert.Equal(HorizontalAlign.Left, opts.PageAlign);
        Assert.Equal(100, opts.Quality);
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog_bw, images[0]);
    }

    [Fact]
    public async Task ScanWithError()
    {
        _bridge.Error = new NoPagesException();

        await Assert.ThrowsAsync<NoPagesException>(async () => await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            PaperSource = PaperSource.Feeder
        }).ToListAsync());
    }

    [Fact]
    public async Task ScanWithErrorAfterPage()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        _bridge.Error = new DeviceException(SdkResources.DevicePaperJam);

        await using var enumerator = _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            PaperSource = PaperSource.Feeder
        }).GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        ImageAsserts.Similar(ImageResources.dog, enumerator.Current);

        var exception = await Assert.ThrowsAsync<DeviceException>(async () => await enumerator.MoveNextAsync());
        Assert.Equal(SdkResources.DevicePaperJam, exception.Message);
    }

    [Fact(Skip = "Flaky")]
    public async Task ScanProgress()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog, ImageResources.dog);
        _bridge.ProgressReports = [0.5];

        var pageStartMock = Substitute.For<EventHandler<PageStartEventArgs>>();
        var pageProgressMock = Substitute.For<EventHandler<PageProgressEventArgs>>();
        _client.PageStart += pageStartMock;
        _client.PageProgress += pageProgressMock;

        await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            PaperSource = PaperSource.Feeder
        }).ToListAsync();

        pageStartMock.Received()(Arg.Any<object>(), Arg.Is<PageStartEventArgs>(args => args.PageNumber == 1));
        // TODO: This flaked and we only got the second one - why? Can we fix it?
        pageProgressMock.Received()(Arg.Any<object>(), Arg.Is<PageProgressEventArgs>(args => args.PageNumber == 1 && args.Progress == 0.5));
        pageStartMock.Received()(Arg.Any<object>(), Arg.Is<PageStartEventArgs>(args => args.PageNumber == 2));
        pageProgressMock.Received()(Arg.Any<object>(), Arg.Is<PageProgressEventArgs>(args => args.PageNumber == 2 && args.Progress == 0.5));
    }
}