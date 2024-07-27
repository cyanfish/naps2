using System.Threading;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NSubstitute;

namespace NAPS2.Sdk.Tests.Mocks;

public class ScanDriverFactoryBuilder
{
    private readonly StubScanDriver _scanDriver;
    private readonly IScanDriverFactory _scanDriverFactory;

    public ScanDriverFactoryBuilder()
    {
        _scanDriver = new StubScanDriver();
        _scanDriverFactory = Substitute.For<IScanDriverFactory>();
        _scanDriverFactory.Create(Arg.Any<ScanOptions>()).Returns(_scanDriver);
    }

    public ScanDriverFactoryBuilder WithDeviceList(params ScanDevice[] devices)
    {
        _scanDriver.DeviceList = devices.ToList();
        return this;
    }

    public ScanDriverFactoryBuilder WithScannedImages(params byte[][] images)
    {
        _scanDriver.AddScanResult(images.Select(image => TestImageContextFactory.Get().Load(image)).ToList());
        return this;
    }

    internal IScanDriverFactory Build()
    {
        return _scanDriverFactory;
    }

    private class StubScanDriver : IScanDriver
    {
        private readonly Queue<List<IMemoryImage>> _scans = new();
            
        public List<ScanDevice> DeviceList { get; set; }

        public void AddScanResult(List<IMemoryImage> images)
        {
            _scans.Enqueue(images);
        }
            
        public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
        {
            foreach (var device in DeviceList)
            {
                callback(device);
            }
            return Task.CompletedTask;
        }

        public Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken)
        {
            return Task.FromResult<ScanCaps>(null);
        }

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
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