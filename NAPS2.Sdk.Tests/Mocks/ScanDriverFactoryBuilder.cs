using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests
{
    public class ScanDriverFactoryBuilder
    {
        private readonly StubScanDriver scanDriver;
        private readonly Mock<IScanDriverFactory> scanDriverFactory;

        public ScanDriverFactoryBuilder()
        {
            scanDriver = new StubScanDriver();
            scanDriverFactory = new Mock<IScanDriverFactory>();
            scanDriverFactory.Setup(x => x.Create(It.IsAny<ScanOptions>())).Returns(scanDriver);
        }

        public ScanDriverFactoryBuilder WithDeviceList(params ScanDevice[] devices)
        {
            scanDriver.DeviceList = devices.ToList();
            return this;
        }

        public ScanDriverFactoryBuilder WithScannedImages(params Bitmap[] images)
        {
            scanDriver.AddScanResult(images.Select(bitmap => (IImage) new GdiImage(bitmap)).ToList());
            return this;
        }

        internal IScanDriverFactory Build()
        {
            return scanDriverFactory.Object;
        }

        private class StubScanDriver : IScanDriver
        {
            private readonly Queue<List<IImage>> scans = new Queue<List<IImage>>();
            
            public List<ScanDevice> DeviceList { get; set; }

            public void AddScanResult(List<IImage> images)
            {
                scans.Enqueue(images);
            }
            
            public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
            {
                return Task.FromResult(DeviceList ?? throw new NotSupportedException());
            }

            public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
            {
                foreach (var image in scans.Dequeue())
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return Task.CompletedTask;
                    }
                    scanEvents.PageStart();
                    callback(image);
                }
                return Task.CompletedTask;
            }
        }
    }
}