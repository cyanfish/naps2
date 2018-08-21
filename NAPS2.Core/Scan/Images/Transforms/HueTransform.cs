using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class HueTransform : Transform
    {
        public int HueShift { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                // No need to handle 1bpp since hue shifts are null transforms
                return bitmap;
            }

            float hueShiftAdjusted = HueShift / 2000f * 360;
            if (hueShiftAdjusted < 0)
            {
                hueShiftAdjusted += 360;
            }

            UnsafeImageOps.HueShift(bitmap, hueShiftAdjusted);
            
            return bitmap;
        }

        public override bool CanSimplify(Transform other) => other is HueTransform;

        public override Transform Simplify(Transform other)
        {
            var other2 = (HueTransform)other;
            return new HueTransform
            {
                HueShift = (HueShift + other2.HueShift + 3000) % 2000 - 1000
            };
        }

        public override bool IsNull => HueShift == 0;
    }
}
