using System.Drawing;
using System.Threading;
using Moq;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests;

public class ScanDriverFactoryBuilder
{
    private readonly StubScanDriver _scanDriver;
    private readonly Mock<IScanDriverFactory> _scanDriverFactory;

    public ScanDriverFactoryBuilder()
    {
        _scanDriver = new StubScanDriver();
        _scanDriverFactory = new Mock<IScanDriverFactory>();
        _scanDriverFactory.Setup(x => x.Create(It.IsAny<ScanOptions>())).Returns(_scanDriver);
    }

    public ScanDriverFactoryBuilder WithDeviceList(params ScanDevice[] devices)
    {
        _scanDriver.DeviceList = devices.ToList();
        return this;
    }

    public ScanDriverFactoryBuilder WithScannedImages(params Bitmap[] images)
    {
        _scanDriver.AddScanResult(images.Select(bitmap => (IImage) new GdiImage(bitmap)).ToList());
        return this;
    }

    internal IScanDriverFactory Build()
    {
        return _scanDriverFactory.Object;
    }

    private class StubScanDriver : IScanDriver
    {
        private readonly Queue<List<IImage>> _scans = new Queue<List<IImage>>();
            
        public List<ScanDevice> DeviceList { get; set; }

        public void AddScanResult(List<IImage> images)
        {
            _scans.Enqueue(images);
        }
            
        public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
        {
            return Task.FromResult(DeviceList ?? throw new NotSupportedException());
        }

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
        {
            foreach (var image in _scans.Dequeue())
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