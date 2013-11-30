using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public class ScannedImageFactory : IScannedImageFactory
    {
        public IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            return new ScannedImage(img, bitDepth, highQuality);
        }
    }
}