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
    [Fact]
    public async Task FindDevice()
    {
        Assert.True(await TryFindClientDevice());
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
        _bridge.Error = new DeviceFeederEmptyException();

        await Assert.ThrowsAsync<DeviceFeederEmptyException>(async () => await _client.Scan(new ScanOptions
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
        pageProgressMock.Received()(Arg.Any<object>(),
            Arg.Is<PageProgressEventArgs>(args => args.PageNumber == 1 && args.Progress == 0.5));
        pageStartMock.Received()(Arg.Any<object>(), Arg.Is<PageStartEventArgs>(args => args.PageNumber == 2));
        pageProgressMock.Received()(Arg.Any<object>(),
            Arg.Is<PageProgressEventArgs>(args => args.PageNumber == 2 && args.Progress == 0.5));
    }

    [Fact]
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
}