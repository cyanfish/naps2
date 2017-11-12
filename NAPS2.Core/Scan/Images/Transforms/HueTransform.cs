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
            double hueShiftAdjusted = (HueShift) / 2000.0 * 360;
            if (hueShiftAdjusted < 0)
            {
                hueShiftAdjusted += 360;
            }

            EnsurePixelFormat(ref bitmap);
            
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var stride = Math.Abs(data.Stride);
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    int r = Marshal.ReadByte(data.Scan0 + stride * y + x * 3);
                    int g = Marshal.ReadByte(data.Scan0 + stride * y + x * 3 + 1);
                    int b = Marshal.ReadByte(data.Scan0 + stride * y + x * 3 + 2);

                    Color c = Color.FromArgb(255, r, g, b);
                    double h, s, v;
                    ColorToHSV(c, out h, out s, out v);

                    h = (h + hueShiftAdjusted) % 360;

                    c = ColorFromHSV(h, s, v);

                    Marshal.WriteByte(data.Scan0 + stride * y + x * 3, c.R);
                    Marshal.WriteByte(data.Scan0 + stride * y + x * 3 + 1, c.G);
                    Marshal.WriteByte(data.Scan0 + stride * y + x * 3 + 2, c.B);
                }
            }
            bitmap.UnlockBits(data);
            
            return bitmap;
        }
        
        // From https://stackoverflow.com/a/1626175
        private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        // From https://stackoverflow.com/a/1626175
        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public override bool IsNull => HueShift == 0;
    }
}
