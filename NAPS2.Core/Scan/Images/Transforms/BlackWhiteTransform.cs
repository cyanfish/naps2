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
            int bytesPerPixel;
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                bytesPerPixel = 3;
            }
            else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                bytesPerPixel = 4;
            }
            else
            {
                return bitmap;
            }

            int thresholdAdjusted = (Threshold + 1000) * 255 / 2;

            var monoBitmap = UnsafeImageOps.ConvertTo1Bpp(bitmap, bytesPerPixel, thresholdAdjusted);
            bitmap.Dispose();

            return monoBitmap;
        }
    }
}
