using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class FileStorageSample
{
    public static async Task Run()
    {
        using ScanningContext scanningContext = new ScanningContext(new GdiImageContext());

        // To save memory, we can store scanned images on disk after initial processing.
        // This will put files in the system temp folder, but you can use any folder.
        scanningContext.FileStorageManager = new FileStorageManager(Path.GetTempPath());

        var controller = new ScanController(scanningContext);
        var device = (await controller.GetDeviceList()).First();
        var options = new ScanOptions
        {
            Device = device
        };

        // We can keep references to every image in the scan and not worry about using an
        // excessive amount of memory, since it is all stored on disk until rendered.
        var images = new List<ProcessedImage>();
        await foreach (var image in controller.Scan(options))
        {
            images.Add(image);
        }

        // Now we can use the images as needed.
        foreach (var image in images)
        {
            // This seamlessly loads the image data from disk.
            using Bitmap bitmap = image.RenderToBitmap();
            // This deletes the data for the individual image from the filesystem.
            image.Dispose();
        }

        // As we declared the ScanningContext with "using", it will now be disposed and delete any remaining data on
        // the filesystem.
    }
}