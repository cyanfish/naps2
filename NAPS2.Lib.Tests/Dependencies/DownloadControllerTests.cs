using Moq;
using NAPS2.Dependencies;
using NAPS2.Sdk.Tests;
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
    private readonly Mock<IExternalComponent> _mockComponent = new();
    private readonly DownloadController _controller;
    private byte[] _downloadData;

    public DownloadControllerTests()
    {
        _mockComponent.Setup(x => x.Install(It.IsAny<string>())).Callback((string p) => _downloadData = File.ReadAllBytes(p));
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

        var mockHandler = new Mock<EventHandler>();

        controller.DownloadComplete += mockHandler.Object;
        Assert.True(await controller.StartDownloadsAsync());

        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
        mockHandler.Verify(x => x(controller, EventArgs.Empty), Times.Once);
    }

    [Fact]
    public async void NoUrl()
    {
        DownloadInfo info = new("", new List<DownloadMirror>(), 0, "0000000000000000000000000000000000000000", DownloadFormat.Gzip);

        var mockHandler = new Mock<Action<string>>();

        _controller.QueueFile(info, mockHandler.Object);

        Assert.False(await _controller.StartDownloadsAsync());

        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
        mockHandler.VerifyNoOtherCalls();
    }

    [Fact]
    public async void InvalidChecksum()
    {
        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        var mockHandler = new Mock<EventHandler>();

        _controller.DownloadError += mockHandler.Object;

        _mockComponent.Setup(x => x.DownloadInfo).Returns(new DownloadInfo("temp.gz", new List<DownloadMirror>() { new DownloadMirror(DummyValidUrl) }, 0, "THIS IS NOT AN SHA1 AND WILL FAIL", DownloadFormat.Gzip));

        _controller.QueueFile(_mockComponent.Object);
        Assert.False(await _controller.StartDownloadsAsync());

        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
        mockHandler.Verify(x => x(_controller, EventArgs.Empty), Times.Once);
        mockHandler.VerifyNoOtherCalls();
        _mockComponent.VerifyGet(x => x.DownloadInfo);
        _mockComponent.VerifyNoOtherCalls();
    }

    [Fact]
    public async void InvalidMirrorsChecksum()
    {
        _mockComponent.Setup(x => x.DownloadInfo).Returns(new DownloadInfo("temp.gz", new List<DownloadMirror>() { new DownloadMirror(DummyInvalidUrl), new DownloadMirror(DummyValidUrl) }, 0, StockDogJpegSHA1, DownloadFormat.Gzip));

        _httpHandler.Expect(DummyInvalidUrl).Respond("application/zip", _animalsZipStream);
        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        _controller.QueueFile(_mockComponent.Object);
        Assert.True(await _controller.StartDownloadsAsync());

        _mockComponent.VerifyGet(x => x.DownloadInfo);
        _mockComponent.Verify(x => x.Install(It.Is((string p) => !string.IsNullOrWhiteSpace(p))));
        _mockComponent.VerifyNoOtherCalls();
        Assert.Equal(BinaryResources.stock_dog, _downloadData);

        _httpHandler.VerifyNoOutstandingExpectation();
        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
    }

    [Fact]
    public async void Valid()
    {
        _mockComponent.Setup(x => x.DownloadInfo).Returns(new DownloadInfo("temp.gz", new List<DownloadMirror>() { new DownloadMirror(DummyValidUrl) }, 0, StockDogJpegSHA1, DownloadFormat.Gzip));

        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        _controller.QueueFile(_mockComponent.Object);
        Assert.True(await _controller.StartDownloadsAsync());

        _mockComponent.VerifyGet(x => x.DownloadInfo);
        _mockComponent.Verify(x => x.Install(It.Is((string p) => !string.IsNullOrWhiteSpace(p))));
        _mockComponent.VerifyNoOtherCalls();
        Assert.Equal(BinaryResources.stock_dog, _downloadData);

        _httpHandler.VerifyNoOutstandingExpectation();
        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
    }

    [Fact]
    public async void ValidUsingMirrorUrl()
    {
        _mockComponent.Setup(x => x.DownloadInfo).Returns(new DownloadInfo("temp.gz", new List<DownloadMirror>() { new DownloadMirror(DummyInvalidUrl), new DownloadMirror(DummyValidUrl) }, 0, StockDogJpegSHA1, DownloadFormat.Gzip));

        _httpHandler.Expect(DummyInvalidUrl);
        _httpHandler.Expect(DummyValidUrl).Respond("application/gzip", _dogsGzipStream);

        _controller.QueueFile(_mockComponent.Object);
        Assert.True(await _controller.StartDownloadsAsync());

        _mockComponent.VerifyGet(x => x.DownloadInfo);
        _mockComponent.Verify(x => x.Install(It.Is((string p) => !string.IsNullOrWhiteSpace(p))));
        _mockComponent.VerifyNoOtherCalls();
        Assert.Equal(BinaryResources.stock_dog, _downloadData);

        _httpHandler.VerifyNoOutstandingExpectation();
        Assert.Equal(0, _httpHandler.GetMatchCount(_httpHandler.Fallback));
    }
}