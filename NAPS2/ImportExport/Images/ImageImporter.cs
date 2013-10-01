using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NLog;

namespace NAPS2.ImportExport.Images
{
    public class ImageImporter : IScannedImageImporter
    {
        private readonly Logger logger;
        private readonly ScannedImageFactory scannedImageFactory;

        public ImageImporter(Logger logger, ScannedImageFactory scannedImageFactory)
        {
            this.logger = logger;
            this.scannedImageFactory = scannedImageFactory;
        }

        public IEnumerable<IScannedImage> Import(string filePath)
        {
            Bitmap toImport;
            try
            {
                toImport = new Bitmap(filePath);
            }
            catch (Exception e)
            {
                logger.ErrorException("Error importing image: " + filePath, e);
                // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                throw;
            }
            using (toImport)
            {
                for (int i = 0; i < toImport.GetFrameCount(FrameDimension.Page); ++i)
                {
                    toImport.SelectActiveFrame(FrameDimension.Page, i);
                    // Disable high quality, since it's too awkward to show a UI and it should be the best choice in most cases
                    yield return scannedImageFactory.Create((Bitmap)toImport.Clone(), ScanBitDepth.C24Bit, false);
                }
            }
        }
    }
}
