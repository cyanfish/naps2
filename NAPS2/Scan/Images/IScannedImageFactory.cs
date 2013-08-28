using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public interface IScannedImageFactory
    {
        IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality);
    }
}
