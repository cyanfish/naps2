using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Scan;
using NAPS2.Scan.Wia;

namespace NAPS2.Sdk.Samples
{
    public class HelloWorldSample
    {
        public static async Task Run()
        {
            // This is the absolute bare bones example of scanning.
            // See the other samples for more description and functionality.

            IScanDriver driver = new WiaScanDriver();
            ScannedImageSource imageSource = driver.Scan(new ScanProfile(), new ScanParams());
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
