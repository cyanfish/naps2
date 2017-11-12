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
            double thresholdAdjusted = Threshold / 1000.0;

            EnsurePixelFormat(ref bitmap);
            
            return bitmap;
        }
    }
}
