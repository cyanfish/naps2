using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
            using (var g = Graphics.FromImage(bitmap))
            {
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix
                {
                    Matrix40 = brightnessAdjusted,
                    Matrix41 = brightnessAdjusted,
                    Matrix42 = brightnessAdjusted
                });
                g.DrawImage(bitmap,
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    0,
                    0,
                    bitmap.Width,
                    bitmap.Height,
                    GraphicsUnit.Pixel,
                    attrs);
            }
            return bitmap;
        }

        public override bool IsNull => Brightness == 0;
    }
}
