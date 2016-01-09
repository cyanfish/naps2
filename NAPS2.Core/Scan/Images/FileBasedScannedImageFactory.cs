using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public class FileBasedScannedImageFactory : IScannedImageFactory
    {
        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality, int quality)
        {
            return new FileBasedScannedImage(img, bitDepth, highQuality, quality);
        }
    }
}