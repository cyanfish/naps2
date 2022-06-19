using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using ZXing.Rendering;

namespace NAPS2.Sdk.Samples;

public class ScanToBitmapSample
{
    public static async Task Run()
    {
        // We configure scanned images to be stored in GDI+ format, which uses
        // System.Drawing.Bitmap internally.
        GdiImageContext imageContext = new GdiImageContext();
        ScanningContext scanningContext = new ScanningContext(imageContext);

        // To select a device and scan, you need a controller.
        ScanController controller = new ScanController(scanningContext);

        // Configure scanning options. There are lots of options - look at the
        // doc for more info.
        ScanOptions options = new ScanOptions
        {
            Dpi = 300
        };

        // Different drivers are supported on different operating systems: Windows supports
        // Wia and Twain, Mac supports Twain, and Linux supports Sane. You can leave this
        // at the default value, but if you want to use Twain on Windows you will need to
        // set it explicitly (Wia is the default on Windows).
        options.Driver = Driver.Twain;
            
        // For the purpose of this sample, we arbitrarily pick the first scanning device.
        // You probably want to let the user select one.
        options.Device = (await controller.GetDeviceList(options)).First();

        // This starts the scan and immediately returns with an object that asynchronously
        // receives the results of the scan.
        ScannedImageSource imageSource = controller.Scan(options);

        // ScannedImageSource has several different methods to help you consume images.
        // ForEach allows you to asynchronously process images as they arrive.
        await imageSource.ForEach(async processedImage =>
        {
            // Make sure ScannedImage and rendered images are disposed after use
            using (processedImage)
            using (Bitmap bitmap = imageContext.RenderToBitmap(processedImage))
            {
                // TODO: Do something with the bitmap
            }
        });
    }
}