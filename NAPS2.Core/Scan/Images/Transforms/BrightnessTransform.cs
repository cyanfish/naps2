using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class BrightnessTransform : Transform
    {
        public int Brightness { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            float brightnessAdjusted = Brightness / 1000f;
            EnsurePixelFormat(ref bitmap);
            UnsafeImageOps.ChangeBrightness(bitmap, brightnessAdjusted);
            return bitmap;
        }

        public override bool IsNull => Brightness == 0;
    }
}
