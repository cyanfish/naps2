using NAPS2.Dependencies;
using NAPS2.Sdk.Tests;
using NSubstitute;
using RichardSzalay.MockHttp;
using Xunit;

namespace NAPS2.Lib.Tests.Dependencies;

public class DownloadControllerTests : ContextualTests
{
    private const string AnimalsSHA1 = "1739ff74f5d26f6c42d7a3fc438d615a93d38bd4";
    private const string StockDogJpegSHA1 = "ca1f89964bdb345e97804784eefa49a00dbf7207";

    private const string DummyValidUrl = "http://localhost/f.zip";
    private const string DummyInvalidUrl = "http://localhost/g.zip";


    private readonly MemoryStream _animalsZipStream = new(BinaryResources.animals);
    private readonly MemoryStream _dogsGzipStream = new(BinaryResources.stock_dog_jpeg);
    private readonly MockHttpMessageHandler _httpHandler = new();
    private readonly IExternalComponent _mockComponent = Substitute.For<IExternalComponent>();
    private readonly DownloadController _controller;
    private byte[] _downloadData;

    public DownloadControllerTests()
    {
        _mockComponent.When(x => x.Install(Arg.Any<string>())).Do(x => _downloadData = File.ReadAllBytes((string) x[0]));
        _controller = new(ScanningContext, _httpHandler.ToHttpClient());
    }

    public override void Dispose()
    {
        _animalsZipStream.Dispose();
        _dogsGzipStream.Dispose();
        base.Dispose();
    }

    [Fact]
    public async void NoQueue()
    {
        MockHttpMessageHandler handler = new();
        DownloadController controller = new(ScanningContext, handler.ToHttpClient());

        var mockHandler = Substitute.For<EventHandler>();

        controller.DownloadComplete += mockHandler;
        Assert.True(await controller.StartDownloadsAsync());

        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
        mockHandler.Received()(controller, EventArgs.Empty);
        mockHandler.ReceivedCallsCount(1);
    }

    [Fact]
    public async void NoUrl()
    {
        DownloadInfo info = new("", [], 0, "0000000000000000000000000000000000000000", DownloadFormat.Gzip);

        var mockHandler = Substitute.For<Action<string>>();

        _controller.QueueFile(info, mockHandler);

        Assert.False(await _controller.StartDownloadsAsync());

        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
        mockHandler.ReceivedCallsCount(0);
    }

    [Fact]
    public async void InvalidChecksum()
    {
        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        var mockHandler = Substitute.For<EventHandler>();

        _controller.DownloadError += mockHandler;

        _mockComponent.DownloadInfo.Returns(new DownloadInfo("temp.gz", [new DownloadMirror(DummyValidUrl)], 0, "THIS IS NOT AN SHA1 AND WILL FAIL", DownloadFormat.Gzip));

        _controller.QueueFile(_mockComponent);
        Assert.False(await _controller.StartDownloadsAsync());

        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
        mockHandler.Received()(_controller, EventArgs.Empty);
        mockHandler.ReceivedCallsCount(1);
        _ = _mockComponent.Received().DownloadInfo;
        _mockComponent.ReceivedCallsCount(1);
    }

    [Fact]
    public async void InvalidMirrorsChecksum()
    {
        _mockComponent.DownloadInfo.Returns(new DownloadInfo("temp.gz",
            [new DownloadMirror(DummyInvalidUrl), new DownloadMirror(DummyValidUrl)], 0, StockDogJpegSHA1, DownloadFormat.Gzip));

        _httpHandler.Expect(DummyInvalidUrl).Respond("application/zip", _animalsZipStream);
        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        _controller.QueueFile(_mockComponent);
        Assert.True(await _controller.StartDownloadsAsync());

        _ = _mockComponent.Received().DownloadInfo;
        _mockComponent.Received().Install(Arg.Is((string p) => !string.IsNullOrWhiteSpace(p)));
        _mockComponent.ReceivedCallsCount(2);
        Assert.Equal(BinaryResources.stock_dog, _downloadData);

        _httpHandler.VerifyNoOutstandingExpectation();
        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
    }

    [Fact]
    public async void Valid()
    {
        _mockComponent.DownloadInfo.Returns(new DownloadInfo("temp.gz", [new DownloadMirror(DummyValidUrl)], 0, StockDogJpegSHA1, DownloadFormat.Gzip));

        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        _controller.QueueFile(_mockComponent);
        Assert.True(await _controller.StartDownloadsAsync());

        _ = _mockComponent.Received().DownloadInfo;
        _mockComponent.Received().Install(Arg.Is((string p) => !string.IsNullOrWhiteSpace(p)));
        _mockComponent.ReceivedCallsCount(2);
        Assert.Equal(BinaryResources.stock_dog, _downloadData);

        _httpHandler.VerifyNoOutstandingExpectation();
        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
    }

    [Fact]
    public async void ValidUsingMirrorUrl()
    {
        _mockComponent.DownloadInfo.Returns(new DownloadInfo("temp.gz",
            [new DownloadMirror(DummyInvalidUrl), new DownloadMirror(DummyValidUrl)], 0, StockDogJpegSHA1, DownloadFormat.Gzip));

        _httpHandler.Expect(DummyInvalidUrl);
        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        _controller.QueueFile(_mockComponent);
        Assert.True(await _controller.StartDownloadsAsync());

        _ = _mockComponent.Received().DownloadInfo;
        _mockComponent.Received().Install(Arg.Is((string p) => !string.IsNullOrWhiteSpace(p)));
        _mockComponent.ReceivedCallsCount(2);
        Assert.Equal(BinaryResources.stock_dog, _downloadData);

        _httpHandler.VerifyNoOutstandingExpectation();
        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
    }
}