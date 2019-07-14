using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Scan.Experimental;

namespace NAPS2.Sdk.Samples
{
    public class FileStorageSample
    {
        public static async Task Run()
        {
            var imageContext = new GdiImageContext();

            // To save memory, we can store scanned images on disk after initial processing.
            // This will put files in the system temp folder by default, which can be
            // overriden by changing FileStorageManager.Current.
            imageContext.ConfigureBackingStorage<FileStorage>();

            var controller = new ScanController();
            var device = controller.GetDeviceList().First();
            var options = new ScanOptions
            {
                Device = device,
                Dpi = 300,
                NoUI = true
            };
            
            // We can wait for the entire scan to complete and not worry about using an
            // excessive amount of memory, since it is all stored on disk until rendered.
            // This is just for illustration purposes; in real code you usually want to
            // process images as they come rather than waiting for the full scan.
            List<ScannedImage> scannedImages = await controller.Scan(options).ToList();

            try
            {
                BitmapRenderer renderer = new BitmapRenderer(imageContext);
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
