#if TEST_SCENARIOS
using System.Threading;
using NAPS2.Images.Bitwise;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.TestScenarios;

/// <summary>
/// Replaces real scanner drivers in test scenarios (unless TestScenarioSetup.RealScanners is set) so no real devices
/// are used.
/// </summary>
internal class FakeScanBridgeFactory(
    IScanBridgeFactory inner, ScanningContext scanningContext, TestScenarioSetup setup) : IScanBridgeFactory
{
    public IScanBridge Create(ScanOptions options) =>
        setup.RealScanners ? inner.Create(options) : new FakeScanBridge(scanningContext, setup);

    private class FakeScanBridge(ScanningContext scanningContext, TestScenarioSetup setup) : IScanBridge
    {
        public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
        {
            await Task.Delay(setup.FakeDeviceDelay, cancelToken);
            foreach (var device in setup.FakeDevices)
            {
                if (device.Driver == options.Driver)
                {
                    callback(device);
                }
            }
        }

        public Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken) =>
            Task.FromResult(new ScanCaps());

        public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
            Action<ProcessedImage, PostProcessingContext> callback)
        {
            foreach (var path in setup.FakeScanImages)
            {
                scanEvents.PageStart();
                await Task.Delay(setup.FakeScanPageDelay, cancelToken);
                var image = path != null
                    ? scanningContext.ImageContext.Load(path)
                    : CreateBlankPage(scanningContext.ImageContext);
                callback(scanningContext.CreateProcessedImage(image), new PostProcessingContext());
            }
        }

        private static IMemoryImage CreateBlankPage(ImageContext imageContext)
        {
            var image = imageContext.Create(850, 1100, ImagePixelFormat.RGB24);
            FillColorImageOp.White.Perform(image);
            image.SetResolution(100, 100);
            return image;
        }
    }
}
#endif
