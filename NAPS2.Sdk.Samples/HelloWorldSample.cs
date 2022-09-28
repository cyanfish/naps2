using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class HelloWorldSample
{
    public static async Task Run()
    {
        // This is the absolute bare bones example of scanning.
        // See the other samples for more description and functionality.

        using var scanningContext = new ScanningContext(new GdiImageContext());
        var controller = new ScanController(scanningContext);
        ScanDevice device = (await controller.GetDeviceList()).First();
        var options = new ScanOptions { Device = device };
        await foreach(var image in controller.Scan(options))
        {
            using (image)
            {
                Console.WriteLine("Scanned a page!");
            }
        }
    }
}