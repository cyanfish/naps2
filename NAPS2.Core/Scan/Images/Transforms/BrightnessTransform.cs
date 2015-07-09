using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Images.Transforms
{
    public class BrightnessTransform : Transform
    {
        private static int Clamp(int brightness)
        {
            return Math.Max(Math.Min(brightness, 1000), -1000);
        }

        public BrightnessTransform()
        {
        }

        public int Brightness { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            float brightnessAdjusted = Brightness / 1000f;

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

        public override bool IsNull
        {
            get { return Brightness == 0; }
        }
    }
}
