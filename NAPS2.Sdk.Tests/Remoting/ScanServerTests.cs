using NAPS2.Escl;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class ScanServerTests(ITestOutputHelper testOutputHelper)
    : ScanServerTestsBase(testOutputHelper, EsclSecurityPolicy.ServerDisableHttps)
{
    [NetworkFact(Timeout = TIMEOUT)]
    public async Task FindDevice()
    {
        Assert.True(await TryFindClientDevice());
    }

    [NetworkFact(Timeout = TIMEOUT)]
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

    [NetworkFact(Timeout = TIMEOUT)]
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

    [NetworkFact(Timeout = TIMEOUT)]
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

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithError()
    {
        _bridge.Error = new DeviceFeederEmptyException();

        await Assert.ThrowsAsync<DeviceFeederEmptyException>(async () => await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            PaperSource = PaperSource.Feeder
        }).ToListAsync());
    }

    [NetworkFact(Timeout = TIMEOUT)]
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

    [NetworkFact(Timeout = TIMEOUT, Skip = "Flaky")]
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
        pageProgressMock.Received()(Arg.Any<object>(),
            Arg.Is<PageProgressEventArgs>(args => args.PageNumber == 1 && args.Progress == 0.5));
        pageStartMock.Received()(Arg.Any<object>(), Arg.Is<PageStartEventArgs>(args => args.PageNumber == 2));
        pageProgressMock.Received()(Arg.Any<object>(),
            Arg.Is<PageProgressEventArgs>(args => args.PageNumber == 2 && args.Progress == 0.5));
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanPreventedByHttpsSecurityPolicy()
    {
        var scanResult = _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            EsclOptions =
            {
                SecurityPolicy = EsclSecurityPolicy.RequireHttps
            }
        });
        await Assert.ThrowsAsync<EsclSecurityPolicyViolationException>(async () => await scanResult.ToListAsync());
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithIpInId()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        await UseServerPort(12145);

        var device = new ScanDevice(Driver.Escl, "http://127.0.0.1:12145/eSCL", _serverDisplayName);
        var images = await _client.Scan(new ScanOptions { Device = device }).ToListAsync();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithCorrectConnectionUri()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        var mockHandler = Substitute.For<EventHandler<DeviceUriChangedEventArgs>>();
        _client.DeviceUriChanged += mockHandler;
        await UseServerPort(12146);

        var device = new ScanDevice(Driver.Escl, "bad_uuid", _serverDisplayName,
            ConnectionUri: "http://127.0.0.1:12146/eSCL");
        var images = await _client.Scan(new ScanOptions { Device = device }).ToListAsync();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
        mockHandler.DidNotReceive()(Arg.Any<object>(), Arg.Any<DeviceUriChangedEventArgs>());
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithIncorrectConnectionUri()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        var mockHandler = Substitute.For<EventHandler<DeviceUriChangedEventArgs>>();
        _client.DeviceUriChanged += mockHandler;
        await UseServerPort(12147);

        var device = new ScanDevice(Driver.Escl, _uuid, _serverDisplayName,
            ConnectionUri: "http://127.0.0.1:31233/eSCL");
        var images = await _client.Scan(new ScanOptions { Device = device }).ToListAsync();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
        mockHandler.Received()(Arg.Any<object>(),
            Arg.Is<DeviceUriChangedEventArgs>(args => args.ConnectionUri.EndsWith(":12147/eSCL")));
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithOfflineDevice()
    {
        var device = new ScanDevice(Driver.Escl, "bad_uuid", _serverDisplayName);
        await Assert.ThrowsAsync<DeviceOfflineException>(async () => await _client.Scan(new ScanOptions
        {
            Device = device,
            EsclOptions = { SearchTimeout = 1000 }
        }).ToListAsync());
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithOfflineDeviceAndIncorrectConnectionUri()
    {
        var device = new ScanDevice(Driver.Escl, "bad_uuid", _serverDisplayName,
            ConnectionUri: "http://127.0.0.1:31233/eSCL");
        await Assert.ThrowsAsync<DeviceOfflineException>(async () => await _client.Scan(new ScanOptions
        {
            Device = device,
            EsclOptions = { SearchTimeout = 1000 }
        }).ToListAsync());
    }

    [NetworkFact(Timeout = TIMEOUT)]
    public async Task ScanWithOfflineDeviceAndIpInId()
    {
        var device = new ScanDevice(Driver.Escl, "http://127.0.0.1:31233/eSCL", _serverDisplayName);
        await Assert.ThrowsAsync<DeviceOfflineException>(async () => await _client.Scan(new ScanOptions
        {
            Device = device
        }).ToListAsync());
    }
}