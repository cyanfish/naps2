using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Twain;
using NAPS2.Util;
using NAPS2.Worker;
using Xunit;

namespace NAPS2.Sdk.Tests.Worker
{
    public class WorkerChannelTests : ContextualTexts
    {
        private Channel Start(ITwainWrapper twainWrapper = null, ThumbnailRenderer thumbnailRenderer = null, IMapiWrapper mapiWrapper = null, ServerCredentials serverCreds = null, ChannelCredentials clientCreds = null)
        {
            Server server = new Server
            {
                Services = { GrpcWorkerService.BindService(new GrpcWorkerServiceImpl(twainWrapper, thumbnailRenderer, mapiWrapper)) },
                Ports = { new ServerPort("localhost", 0, serverCreds ?? ServerCredentials.Insecure) }
            };
            server.Start();
            var client = new GrpcWorkerServiceAdapter(server.Ports.First().BoundPort, clientCreds ?? ChannelCredentials.Insecure);
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
            var serverCreds = GrpcHelper.GetServerCreds(cert, privateKey);
            var clientCreds = GrpcHelper.GetClientCreds(cert, privateKey);
            using (var channel = Start(serverCreds: serverCreds, clientCreds: clientCreds))
            {
                channel.Client.Init(null);
            }
        }

        [Fact]
        public void Init()
        {
            using (var channel = Start())
            {
                channel.Client.Init(@"C:\Somewhere");
                Assert.IsType<RecoveryStorageManager>(FileStorageManager.Current);
                Assert.StartsWith(@"C:\Somewhere", ((RecoveryStorageManager)FileStorageManager.Current).RecoveryFolderPath);
            }
        }

        [Fact]
        public void Wia10NativeUi()
        {
            // TODO: This is not testable yet
            // channel.Client.Wia10NativeUI(...);
        }

        [Fact]
        public void TwainGetDeviceList()
        {
            var twainWrapper = new Mock<ITwainWrapper>();
            using (var channel = Start(twainWrapper.Object))
            {
                twainWrapper
                    .Setup(tw => tw.GetDeviceList(TwainImpl.OldDsm))
                    .Returns(new List<ScanDevice> { new ScanDevice("test_id", "test_name") });

                var deviceList = channel.Client.TwainGetDeviceList(TwainImpl.OldDsm);

                Assert.Single(deviceList);
                Assert.Equal("test_id", deviceList[0].ID);
                Assert.Equal("test_name", deviceList[0].Name);
                twainWrapper.Verify(tw => tw.GetDeviceList(TwainImpl.OldDsm));
                twainWrapper.VerifyNoOtherCalls();
            }
        }

        [Fact]
        public async Task TwainScanWithMemoryStorage()
        {
            StorageManager.ConfigureBackingStorage<GdiImage>();
            await TwainScanInternalTest();
        }

        [Fact]
        public async Task TwainScanWithFileStorage()
        {
            UseFileStorage();
            await TwainScanInternalTest();
        }

        [Fact]
        public async Task TwainScanWithRecovery()
        {
            UseRecovery();
            await TwainScanInternalTest();
        }

        private async Task TwainScanInternalTest()
        {
            var twainWrapper = new TwainWrapperMockScanner
            {
                Images = new List<ScannedImage>
                {
                    CreateScannedImage(),
                    CreateScannedImage()
                }
            };

            using (var channel = Start(twainWrapper))
            {
                var receivedImages = new List<ScannedImage>();
                await channel.Client.TwainScan(new ScanDevice("test_id", "test_name"), new ScanProfile(), new ScanParams(), IntPtr.Zero,
                    CancellationToken.None,
                    (img, path) => { receivedImages.Add(img); });

                Assert.Equal(2, receivedImages.Count);
            }
        }

        [Fact]
        public async Task TwainScanException()
        {
            var twainWrapper = new TwainWrapperMockScanner
            {
                Exception = new DeviceException("Test error")
            };
            using (var channel = Start(twainWrapper))
            {
                var ex = await Assert.ThrowsAsync<DeviceException>(async () => await channel.Client.TwainScan(
                    new ScanDevice("test_id", "test_name"),
                    new ScanProfile(),
                    new ScanParams(),
                    IntPtr.Zero,
                    CancellationToken.None,
                    (img, path) => { }));
                Assert.Contains(nameof(TwainWrapperMockScanner), ex.StackTrace);
                Assert.Contains("Test error", ex.Message);
            }
        }

        private ScannedImage CreateScannedImage()
        {
            return new ScannedImage(new GdiImage(new Bitmap(100, 100)));
        }

        private class TwainWrapperMockScanner : ITwainWrapper
        {
            public List<ScannedImage> Images { get; set; } = new List<ScannedImage>();

            public Exception Exception { get; set; }

            public List<ScanDevice> GetDeviceList(TwainImpl twainImpl) => throw new NotSupportedException();

            public void Scan(IntPtr dialogParent, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, CancellationToken cancelToken, ScannedImageSink sink,
                Action<ScannedImage, ScanParams, string> runBackgroundOcr)
            {
                foreach (var img in Images)
                {
                    sink.PutImage(img);
                }

                if (Exception != null)
                {
                    throw Exception;
                }
            }
        }

        private class Channel : IDisposable
        {
            public Server Server { get; set; }

            public GrpcWorkerServiceAdapter Client { get; set; }

            public void Dispose()
            {
                Server.KillAsync();
            }
        }
    }
}
