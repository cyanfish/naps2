using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport.Images
{
    public class ImageImporter : IImageImporter
    {
        private readonly ScannedImageFactory scannedImageFactory;

        public ImageImporter(ScannedImageFactory scannedImageFactory)
        {
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
                Log.ErrorException("Error importing image: " + filePath, e);
                // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                throw;
            }
            using (toImport)
            {
                for (int i = 0; i < toImport.GetFrameCount(FrameDimension.Page); ++i)
                {
                    toImport.SelectActiveFrame(FrameDimension.Page, i);
                    yield return scannedImageFactory.Create((Bitmap)toImport.Clone(), ScanBitDepth.C24Bit, IsLossless(toImport.RawFormat));
                }
            }
        }

        private bool IsLossless(ImageFormat format)
        {
            return Equals(format, ImageFormat.Bmp) || Equals(format, ImageFormat.Png);
        }
    }
}
