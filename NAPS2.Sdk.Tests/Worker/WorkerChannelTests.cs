using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Moq;
using NAPS2.Images;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Worker;
using Xunit;

namespace NAPS2.Sdk.Tests.Worker
{
    public class WorkerChannelTests
    {
        private Channel Start()
        {
            var twainWrapper = new Mock<ITwainWrapper>();
            var thumbnailRenderer = new ThumbnailRenderer();
            var mapiWrapper = new Mock<IMapiWrapper>();
            Server server = new Server
            {
                Services = { GrpcWorkerService.BindService(new GrpcWorkerServiceImpl(twainWrapper.Object, thumbnailRenderer, mapiWrapper.Object)) },
                Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) }
            };
            server.Start();
            var client = new GrpcWorkerServiceAdapter(server.Ports.First().BoundPort);
            return new Channel
            {
                Server = server,
                Client = client,
                TwainWrapper = twainWrapper,
                ThumbnailRenderer = thumbnailRenderer,
                MapiWrapper = mapiWrapper
            };
        }

        [Fact]
        public void Init()
        {
            using (var channel = Start())
            {
                // TODO: When init actually does something, do a better test
                channel.Client.Init("");
            }
        }

        [Fact]
        public void TwainGetDeviceList()
        {
            using (var channel = Start())
            {
                channel.TwainWrapper
                    .Setup(tw => tw.GetDeviceList(TwainImpl.OldDsm))
                    .Returns(new List<ScanDevice> { new ScanDevice("test_id", "test_name") });

                var deviceList = channel.Client.TwainGetDeviceList(TwainImpl.OldDsm);

                Assert.Single(deviceList);
                Assert.Equal("test_id", deviceList[0].ID);
                Assert.Equal("test_name", deviceList[0].Name);
                channel.TwainWrapper.Verify(tw => tw.GetDeviceList(TwainImpl.OldDsm));
                channel.TwainWrapper.VerifyNoOtherCalls();
            }
        }

        private class Channel : IDisposable
        {
            public Server Server { get; set; }

            public GrpcWorkerServiceAdapter Client { get; set; }

            public Mock<ITwainWrapper> TwainWrapper { get; set; }

            public ThumbnailRenderer ThumbnailRenderer { get; set; }

            public Mock<IMapiWrapper> MapiWrapper { get; set; }

            public void Dispose()
            {
                Server.KillAsync();
            }
        }
    }
}
