using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class HueTransform : Transform
    {
        public int HueShift { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            double hueShiftAdjusted = HueShift / 1000.0;

            EnsurePixelFormat(ref bitmap);
            
            return bitmap;
        }

        public override bool IsNull => HueShift == 0;
    }
}
