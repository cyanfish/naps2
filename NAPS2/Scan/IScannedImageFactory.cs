using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    public interface IScannedImageFactory
    {
        IScannedImage Create(Bitmap img, ScanBitDepth bitDepth, bool highQuality);
    }
}
