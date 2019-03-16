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
using NAPS2.Scan.Twain;
using NAPS2.Worker;
using Xunit;

namespace NAPS2.Sdk.Tests.Worker
{
    public class WorkerChannelTests
    {
        private Channel Start(ITwainWrapper twainWrapper = null, ThumbnailRenderer thumbnailRenderer = null, IMapiWrapper mapiWrapper = null)
        {
            Server server = new Server
            {
                Services = { GrpcWorkerService.BindService(new GrpcWorkerServiceImpl(twainWrapper, thumbnailRenderer, mapiWrapper)) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();
            var client = new GrpcWorkerServiceAdapter(server.Ports.First().BoundPort);
            return new Channel
            {
                Server = server,
                Client = client
            };
        }

        [Fact]
        public void Init()
        {
            using (var channel = Start())
            {
                var fsm = FileStorageManager.Current;
                try
                {
                    channel.Client.Init(@"C:\Somewhere");
                    Assert.IsType<RecoveryStorageManager>(FileStorageManager.Current);
                    Assert.StartsWith(@"C:\Somewhere", ((RecoveryStorageManager)FileStorageManager.Current).RecoveryFolderPath);
                }
                finally
                {
                    FileStorageManager.Current = fsm;
                }
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
        public async Task TwainScan()
        {
            ScannedImage.ConfigureBackingStorage<IFileStorage>();
            try
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
            finally
            {
                ScannedImage.ConfigureBackingStorage<GdiImage>();
            }
        }

        private ScannedImage CreateScannedImage()
        {
            return new ScannedImage(new GdiImage(new Bitmap(100, 100)));
        }

        private class TwainWrapperMockScanner : ITwainWrapper
        {
            public List<ScannedImage> Images { get; set; } = new List<ScannedImage>();

            public List<ScanDevice> GetDeviceList(TwainImpl twainImpl) => throw new NotSupportedException();

            public void Scan(IntPtr dialogParent, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, CancellationToken cancelToken, ScannedImageSink sink,
                Action<ScannedImage, ScanParams, string> runBackgroundOcr)
            {
                foreach (var img in Images)
                {
                    sink.PutImage(img);
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
