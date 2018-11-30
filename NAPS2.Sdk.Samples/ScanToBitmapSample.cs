using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Scan.Wia;

namespace NAPS2.Sdk.Samples
{
    public class ScanToBitmapSample
    {
        public static async IAsyncEnumerable<Bitmap> Run()
        {
            // TODO: Configure image factory.
            // We configure scanned images to be stored in GDI+ format.
            // This uses System.Drawing.Bitmap internally.
            ScannedImage.ConfigureBackingStorage<GdiImage>();
            // TODO: Put this in another sample.
            // Alternatively, we could store images on disk to save memory.
            // This would put files in the system temp folder by default,
            // which can be overriden by changing FileStorageManager.Current.
            // ScannedImage.ConfigureBackingStorage<IFileStorage>();

            // To select a device and scan, you need a driver.
            // Windows supports Wia and Twain. Linux supports Sane. Mac supports Twain.
            // You can use the IScanDriver.IsSupported property to help choose if needed.
            IScanDriver driver = new WiaScanDriver();

            // For the purpose of this sample, we arbitrarily pick the first scanning device.
            // You probably want to let the user select one. Use IScanDriver.PromptForDevice()
            // to show a device prompt. This may use Windows Forms for some drivers.
            ScanDevice device = driver.GetDeviceList().First();

            // Configure basic scanning options (these are usually presented to the user)
            driver.ScanProfile = new ScanProfile
            {
                Device = device,
                Resolution = ScanDpi.Dpi300
            };
            
            // Configure meta scanning options
            driver.ScanParams = new ScanParams
            {
                NoUI = true
            };

            // This starts the scan and immediately returns with an object that asynchronously
            // receives the results of the scan.
            ScannedImageSource imageSource = driver.Scan();

            // To change a ScannedImage object into a Bitmap object we need a renderer.
            // Since our backing storage already uses a Bitmap, this is a fairly trivial operation.
            // However, in other situations this abstracts away additional I/O or transforms.
            BitmapRenderer renderer = new BitmapRenderer();

            // Using the new C# 8.0 language features and IAsyncEnumerable allows this to be
            // done cleanly and fully asynchronously.
            await foreach (ScannedImage image in imageSource.AsAsyncEnumerable())
            {
                Bitmap bitmap = await renderer.Render(image);
                yield return bitmap;
            }
        }
    }
}
