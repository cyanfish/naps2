using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Remoting;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;
using NAPS2.Util;
using Xunit;

namespace NAPS2.Sdk.Tests.Worker
{
    public class WorkerChannelTests : ContextualTexts
    {
        private Channel Start(IRemoteScanController remoteScanController = null, ThumbnailRenderer thumbnailRenderer = null, IMapiWrapper mapiWrapper = null, ServerCredentials serverCreds = null, ChannelCredentials clientCreds = null)
        {
            Server server = new Server
            {
                Services = { WorkerService.BindService(new WorkerServiceImpl(ImageContext, remoteScanController, thumbnailRenderer, mapiWrapper)) },
                Ports = { new ServerPort("localhost", 0, serverCreds ?? ServerCredentials.Insecure) }
            };
            server.Start();
            var client = new WorkerServiceAdapter(server.Ports.First().BoundPort, clientCreds ?? ChannelCredentials.Insecure);
            return new Channel
            {
                Server = server,
                Client = client
            };
        }

        [Fact]
        public void SslCreds()
        {
            var (cert, privateKey) = SslHelper.GenerateRootCertificate();
            var serverCreds = RemotingHelper.GetServerCreds(cert, privateKey);
            var clientCreds = RemotingHelper.GetClientCreds(cert, privateKey);
            using var channel = Start(serverCreds: serverCreds, clientCreds: clientCreds);
            channel.Client.Init(null);
        }

        [Fact]
        public void Init()
        {
            using var channel = Start();
            channel.Client.Init(@"C:\Somewhere");
            Assert.IsType<RecoveryStorageManager>(ImageContext.FileStorageManager);
            Assert.StartsWith(@"C:\Somewhere", ((RecoveryStorageManager)ImageContext.FileStorageManager).RecoveryFolderPath);
        }

        [Fact]
        public void Wia10NativeUi()
        {
            // TODO: This is not testable yet
            // channel.Client.Wia10NativeUI(...);
        }

        [Fact]
        public async Task GetDeviceList()
        {
            var remoteScanController = new Mock<IRemoteScanController>();
            using var channel = Start(remoteScanController.Object);
            remoteScanController
                .Setup(rsc => rsc.GetDeviceList(It.IsAny<ScanOptions>()))
                .ReturnsAsync(new List<ScanDevice> { new ScanDevice("test_id", "test_name") });

            var deviceList = await channel.Client.GetDeviceList(new ScanOptions());

            Assert.Single(deviceList);
            Assert.Equal("test_id", deviceList[0].ID);
            Assert.Equal("test_name", deviceList[0].Name);
            remoteScanController.Verify(rsc => rsc.GetDeviceList(It.IsAny<ScanOptions>()));
            remoteScanController.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ScanWithMemoryStorage()
        {
            ImageContext.ConfigureBackingStorage<GdiImage>();
            await ScanInternalTest();
        }

        [Fact]
        public async Task ScanWithFileStorage()
        {
            UseFileStorage();
            await ScanInternalTest();
        }

        [Fact]
        public async Task ScanWithRecovery()
        {
            UseRecovery();
            await ScanInternalTest();
        }

        private async Task ScanInternalTest()
        {
            var remoteScanController = new MockRemoteScanController
            {
                Images = new List<ScannedImage>
                {
                    CreateScannedImage(),
                    CreateScannedImage()
                }
            };

            using var channel = Start(remoteScanController);
            var receivedImages = new List<ScannedImage>();
            await channel.Client.Scan(ImageContext, new ScanOptions(),
                CancellationToken.None, new ScanEvents(() => { }, _ => { }), 
                (img, path) => { receivedImages.Add(img); });

            Assert.Equal(2, receivedImages.Count);
        }

        [Fact]
        public async Task ScanException()
        {
            var remoteScanController = new MockRemoteScanController
            {
                Images = new List<ScannedImage>
                {
                    CreateScannedImage(),
                    CreateScannedImage()
                },
                Exception = new DeviceException("Test error")
            };
            using var channel = Start(remoteScanController);
            var ex = await Assert.ThrowsAsync<DeviceException>(async () => await channel.Client.Scan(
                ImageContext,
                new ScanOptions(),
                CancellationToken.None,
                new ScanEvents(() => { }, _ => { }), 
                (img, path) => { }));
            Assert.Contains(nameof(MockRemoteScanController), ex.StackTrace);
            Assert.Contains("Test error", ex.Message);
        }

        private class MockRemoteScanController : IRemoteScanController
        {
            public List<ScannedImage> Images { get; set; } = new List<ScannedImage>();

            public Exception Exception { get; set; }

            public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) => throw new NotSupportedException();

            public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback)
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
            public Server Server { get; set; }

            public WorkerServiceAdapter Client { get; set; }

            public void Dispose()
            {
                Server.KillAsync();
            }
        }
    }
}
