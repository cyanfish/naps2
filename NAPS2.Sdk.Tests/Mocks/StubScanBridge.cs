using System.Threading;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests.Mocks;

internal class StubScanBridge : IScanBridge
{
    public List<ScanDevice> MockDevices { get; set; } = new List<ScanDevice>();

    public List<ScannedImage> MockOutput { get; set; } = new List<ScannedImage>();
        
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

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback)
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