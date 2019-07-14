using System;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples
{
    public class HelloWorldSample
    {
        public static async Task Run()
        {
            // This is the absolute bare bones example of scanning.
            // See the other samples for more description and functionality.

            ScanController controller = new ScanController();
            ScanDevice device = (await controller.GetDeviceList()).First();
            ScanOptions options = new ScanOptions { Device = device };
            ScannedImageSource imageSource = controller.Scan(options);
            await imageSource.ForEach(scannedImage =>
            {
                using (scannedImage)
                {
                    Console.WriteLine("Scanned a page!");
                }
            });
        }
    }
}
