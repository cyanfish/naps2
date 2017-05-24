using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    public class TrueContrastTransform : Transform
    {
        public int Contrast { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            // convert +/-1000 input range to a logarithmic scaled multiplier
            float contrastAdjusted = (float) Math.Pow(2.718281f, Contrast / 500.0f);
            // see http://docs.rainmeter.net/tips/colormatrix-guide/ for offset & matrix calculation
            float offset = (1.0f - contrastAdjusted) / 2.0f;

            EnsurePixelFormat(ref bitmap);
            using (var g = Graphics.FromImage(bitmap))
            {
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix
                {
                    Matrix00 = contrastAdjusted,
                    Matrix11 = contrastAdjusted,
                    Matrix22 = contrastAdjusted,
                    Matrix33 = 1.0f,
                    Matrix44 = 1.0f,
                    Matrix40 = offset,
                    Matrix41 = offset,
                    Matrix42 = offset,
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
