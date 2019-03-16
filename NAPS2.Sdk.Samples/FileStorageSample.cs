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
    public class FileStorageSample
    {
        public static async Task Run()
        {
            StorageManager.ConfigureImageType<GdiImage>();

            // To save memory, we can store scanned images on disk after initial processing.
            // This will put files in the system temp folder by default, which can be
            // overriden by changing FileStorageManager.Current.
            // TODO: Probably this should go on StorageManager? It's weird to have to remember what's where
            ScannedImage.ConfigureBackingStorage<IFileStorage>();
            
            IScanDriver driver = new WiaScanDriver();
            ScanDevice device = driver.GetDeviceList().First();
            ScanProfile scanProfile = new ScanProfile
            {
                Device = device,
                Resolution = ScanDpi.Dpi300
            };
            ScanParams scanParams = new ScanParams
            {
                NoUI = true
            };
            
            // We can wait for the entire scan to complete and not worry about using an
            // excessive amount of memory, since it is all stored on disk until rendered.
            // This is just for illustration purposes; in real code you usually want to
            // process images as they come rather than waiting for the full scan.
            List<ScannedImage> scannedImages = await driver.Scan(scanProfile, scanParams).ToList();

            try
            {
                BitmapRenderer renderer = new BitmapRenderer();
                foreach (var scannedImage in scannedImages)
                {
                    // This seamlessly loads the image data from disk.
                    using (Bitmap bitmap = await renderer.Render(scannedImage))
                    {
                        // TODO: Do something with the bitmap
                    }
                }
            }
            finally
            {
                foreach (var scannedImage in scannedImages)
                {
                    // This cleanly deletes any data from the filesystem.
                    scannedImage.Dispose();
                }
            }
        }
    }
}
