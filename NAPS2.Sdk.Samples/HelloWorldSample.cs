using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class HelloWorldSample
{
    public static async Task Run()
    {
        // This is the absolute bare bones example of scanning.
        // See the other samples for more description and functionality.

        using ScanningContext scanningContext = new ScanningContext(new GdiImageContext());
        ScanController controller = new ScanController(scanningContext);
        ScanDevice device = (await controller.GetDeviceList()).First();
        ScanOptions options = new ScanOptions { Device = device };
        AsyncSource<ProcessedImage> imageSource = controller.Scan(options);
        await imageSource.ForEach(scannedImage =>
        {
            using (scannedImage)
            {
                Console.WriteLine("Scanned a page!");
            }
        });
    }
}