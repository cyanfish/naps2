using System.Threading;
using GrpcDotNetNamedPipes;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Twain;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Worker;

public class WorkerChannelTests : ContextualTests
{
    private Channel Start(IRemoteScanController remoteScanController = null, ThumbnailRenderer thumbnailRenderer = null,
        IMapiWrapper mapiWrapper = null, ITwainSessionController twainSessionController = null)
    {
        string pipeName = $"WorkerNamedPipeTests.{Path.GetRandomFileName()}";
        NamedPipeServer server = new NamedPipeServer(pipeName);
        WorkerService.BindService(server.ServiceBinder,
            new WorkerServiceImpl(ScanningContext, remoteScanController, thumbnailRenderer, mapiWrapper,
                twainSessionController));
        server.Start();
        var client = new WorkerServiceAdapter(new NamedPipeChannel(".", pipeName));
        return new Channel
        {
            Server = server,
            Client = client
        };
    }

    [Fact]
    public void Init()
    {
        using var channel = Start();
        channel.Client.Init(@"C:\Somewhere");
        Assert.StartsWith(@"C:\Somewhere", ScanningContext.FileStorageManager.NextFilePath());
    }

    [Fact]
    public void Wia10NativeUi()
    {
        // TODO: This is not testable yet
        // channel.Client.Wia10NativeUI(...);
    }

    [Fact]
    public async Task GetDevices()
    {
        var remoteScanController = Substitute.For<IRemoteScanController>();
        using var channel = Start(remoteScanController);
        remoteScanController.GetDevices(Arg.Any<ScanOptions>(), Arg.Any<CancellationToken>(),
                Arg.Any<Action<ScanDevice>>())
            .Returns(x =>
            {
                var callback = (Action<ScanDevice>) x[2];
                callback(new ScanDevice(Driver.Wia, "test_id", "test_name"));
                return Task.CompletedTask;
            });

        var deviceList = new List<ScanDevice>();
        await channel.Client.GetDevices(new ScanOptions(), CancellationToken.None, deviceList.Add);

        Assert.Single(deviceList);
        Assert.Equal("test_id", deviceList[0].ID);
        Assert.Equal("test_name", deviceList[0].Name);
        _ = remoteScanController.Received().GetDevices(Arg.Any<ScanOptions>(), Arg.Any<CancellationToken>(),
            Arg.Any<Action<ScanDevice>>());
        remoteScanController.ReceivedCallsCount(1);
    }

    [Fact]
    public async Task ScanWithMemoryStorage()
    {
        await ScanInternalTest();
    }

    [Fact]
    public async Task ScanWithFileStorage()
    {
        SetUpFileStorage();
        await ScanInternalTest();
    }

    private async Task ScanInternalTest()
    {
        var remoteScanController = new MockRemoteScanController
        {
            Images =
            [
                CreateScannedImage(),
                CreateScannedImage()
            ]
        };

        using var channel = Start(remoteScanController);
        var receivedImages = new List<ProcessedImage>();
        await channel.Client.Scan(
            ScanningContext,
            new ScanOptions(),
            CancellationToken.None,
            ScanEvents.Stub,
            (img, path) => { receivedImages.Add(img); });

        Assert.Equal(2, receivedImages.Count);
        // TODO: Verify that thumbnails are set correctly (with and without revertible transforms)
    }

    [Fact]
    public async Task ScanException()
    {
        var remoteScanController = new MockRemoteScanController
        {
            Images =
            [
                CreateScannedImage(),
                CreateScannedImage()
            ],
            Exception = new DeviceException("Test error")
        };
        using var channel = Start(remoteScanController);
        var ex = await Assert.ThrowsAsync<DeviceException>(async () => await channel.Client.Scan(
            ScanningContext,
            new ScanOptions(),
            CancellationToken.None,
            ScanEvents.Stub,
            (img, path) => { }));
        Assert.Contains(nameof(MockRemoteScanController), ex.StackTrace);
        Assert.Contains("Test error", ex.Message);
    }

    [Fact]
    public async Task TwainScan()
    {
        var twainEvents = Substitute.For<ITwainEvents>();
        var sessionController = Substitute.For<ITwainSessionController>();

        sessionController.StartScan(Arg.Any<ScanOptions>(), Arg.Any<TwainEvents>(), Arg.Any<CancellationToken>())
            .Returns(
                x =>
                {
                    var serverTwainEvents = (ITwainEvents) x[1];
                    serverTwainEvents.PageStart(new TwainPageStart());
                    serverTwainEvents.MemoryBufferTransferred(new TwainMemoryBuffer());
                    serverTwainEvents.PageStart(new TwainPageStart());
                    serverTwainEvents.NativeImageTransferred(new TwainNativeImage());
                    return Task.CompletedTask;
                });

        using var channel = Start(twainSessionController: sessionController);
        await channel.Client.TwainScan(new ScanOptions(), CancellationToken.None, twainEvents);

        twainEvents.Received().PageStart(Arg.Any<TwainPageStart>());
        twainEvents.Received().MemoryBufferTransferred(Arg.Any<TwainMemoryBuffer>());
        twainEvents.Received().PageStart(Arg.Any<TwainPageStart>());
        twainEvents.Received().NativeImageTransferred(Arg.Any<TwainNativeImage>());
        twainEvents.ReceivedCallsCount(4);
    }

    private class MockRemoteScanController : IRemoteScanController
    {
        public List<ProcessedImage> Images { get; set; } = [];

        public Exception Exception { get; set; }

        public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback) =>
            throw new NotSupportedException();

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
            Action<ProcessedImage, PostProcessingContext> callback)
        {
            return Task.Run(() =>
            {
                foreach (var img in Images)
                {
                    callback(img, new PostProcessingContext());
                }

                if (Exception != null)
                {
                    throw Exception;
                }
            });
        }
    }

    private class Channel : IDisposable
    {
        public NamedPipeServer Server { get; set; }

        public WorkerServiceAdapter Client { get; set; }

        public void Dispose()
        {
            Server.Kill();
        }
    }
}