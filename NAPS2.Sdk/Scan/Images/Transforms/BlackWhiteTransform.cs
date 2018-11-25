using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class BlackWhiteTransform : Transform
    {
        public int Threshold { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                return bitmap;
            }

            var monoBitmap = UnsafeImageOps.ConvertTo1Bpp(bitmap, Threshold);
            bitmap.Dispose();

            return monoBitmap;
        }
    }
}
