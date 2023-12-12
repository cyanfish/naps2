using System.Threading;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests.Mocks;

internal class MockScanBridge : IScanBridge
{
    public List<ScanDevice> MockDevices { get; set; } = [];

    public List<ProcessedImage> MockOutput { get; set; } = [];
        
    public Exception Error { get; set; }

    public ScanOptions LastOptions { get; private set; }

    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        LastOptions = options;
        return Task.Run(() =>
        {
            if (Error != null)
            {
                throw Error;
            }

            foreach (var device in MockDevices)
            {
                callback(device);
            }
        });
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ProcessedImage, PostProcessingContext> callback)
    {
        LastOptions = options;
        return Task.Run(() =>
        {
            foreach (var img in MockOutput)
            {
                callback(img.Clone(), new PostProcessingContext());
            }
            if (Error != null)
            {
                throw Error;
            }
        });
    }
}