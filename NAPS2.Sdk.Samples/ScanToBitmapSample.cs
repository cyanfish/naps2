using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class ScanToBitmapSample
{
    public static async Task Run()
    {
        // We configure scanned images to be stored in GDI+ format, which uses
        // System.Drawing.Bitmap internally.
        using var scanningContext = new ScanningContext(new GdiImageContext());

        // To select a device and scan, you need a controller.
        var controller = new ScanController(scanningContext);

        // Different drivers are supported on different operating systems: Windows supports
        // Wia and Twain, Mac supports Apple and Sane, and Linux supports Sane. You can leave this
        // at the default value, but if you want to use Twain on Windows you will need to
        // set it explicitly (Wia is the default on Windows).
        var driver = Driver.Twain;

        // For the purpose of this sample, we arbitrarily pick the first scanning device.
        // You probably want to let the user select one.
        ScanDevice device = (await controller.GetDeviceList(driver)).First();

        // Configure scanning options. There are lots of options - look at the
        // doc for more info.
        var options = new ScanOptions
        {
            Dpi = 300,
            Driver = driver,
            Device = device
        };

        // We can now start the scan and asynchronously enumerate over the scanned pages.
        await foreach (ProcessedImage image in controller.Scan(options))
        {
            using Bitmap bitmap = image.RenderToBitmap();
            // You probably also want to dispose the ProcessedImage when you're done with it.
            // Alternatively, you can rely on disposing ScanningContext, which will also dispose all derived
            // ProcessedImage objects.
            image.Dispose();
        }
    }
}