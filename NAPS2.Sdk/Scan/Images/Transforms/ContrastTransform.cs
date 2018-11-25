using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class ContrastTransform : Transform
    {
        public int Contrast { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            float contrastAdjusted = Contrast / 1000f + 1.0f;

            EnsurePixelFormat(ref bitmap);
            using (var g = Graphics.FromImage(bitmap))
            {
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix
                {
                    Matrix00 = contrastAdjusted,
                    Matrix11 = contrastAdjusted,
                    Matrix22 = contrastAdjusted
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

        public override bool IsNull => Contrast == 0;
    }
}
