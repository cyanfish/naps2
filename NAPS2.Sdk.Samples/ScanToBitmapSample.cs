using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Scan.Wia;

namespace NAPS2.Sdk.Samples
{
    public class ScanToBitmapSample
    {
        public static async Task Run()
        {
            // We configure scanned images to be stored in GDI+ format, which uses
            // System.Drawing.Bitmap internally.
            var imageContext = new GdiImageContext();

            // To select a device and scan, you need a driver.
            // Windows supports Wia and Twain. Linux supports Sane. Mac supports Twain.
            // You can use the IScanDriver.IsSupported property to help choose if needed.
            IScanDriver driver = new WiaScanDriver();

            // For the purpose of this sample, we arbitrarily pick the first scanning device.
            // You probably want to let the user select one. Use IScanDriver.PromptForDevice()
            // to show a device prompt. This may start a Windows Forms event loop.
            ScanDevice device = driver.GetDeviceList().First();

            // Configure basic scanning options (these are usually presented to the user)
            ScanProfile scanProfile = new ScanProfile
            {
                Device = device,
                Resolution = ScanDpi.Dpi300
            };

            // Configure meta scanning options
            ScanParams scanParams = new ScanParams
            {
                NoUI = true
            };

            // This starts the scan and immediately returns with an object that asynchronously
            // receives the results of the scan.
            ScannedImageSource imageSource = driver.Scan(scanProfile, scanParams);

            // To change a ScannedImage object into a Bitmap object we need a renderer.
            // Since our backing storage already uses a Bitmap, this is a fairly trivial operation.
            // However, in other situations this abstracts away additional I/O or transforms.
            BitmapRenderer renderer = new BitmapRenderer(imageContext);

            // ScannedImageSource has several different methods to help you consume images.
            // ForEach allows you to asynchronously process images as they arrive.
            await imageSource.ForEach(async scannedImage =>
            {
                // Make sure ScannedImage and rendered images are disposed after use
                using (scannedImage)
                using (Bitmap bitmap = await renderer.Render(scannedImage))
                {
                    // TODO: Do something with the bitmap
                }
            });
        }
    }
}
