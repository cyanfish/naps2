using System.Threading;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests.Mocks;

internal class StubScanBridge : IScanBridge
{
    public List<ScanDevice> MockDevices { get; set; } = new();

    public List<ProcessedImage> MockOutput { get; set; } = new();
        
    public Exception Error { get; set; }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        return Task.Run(() =>
        {
            if (Error != null)
            {
                throw Error;
            }

            return MockDevices;
        });
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ProcessedImage, PostProcessingContext> callback)
    {
        return Task.Run(() =>
        {
            foreach (var img in MockOutput)
            {
                callback(img, new PostProcessingContext());
            }
            if (Error != null)
            {
                throw Error;
            }
        });
    }
}