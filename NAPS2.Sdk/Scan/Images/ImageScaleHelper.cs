using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    internal static class ImageScaleHelper
    {
        public static Bitmap ScaleImage(Image original, double scaleFactor)
        {
            double realWidth = original.Width / scaleFactor;
            double realHeight = original.Height / scaleFactor;

            double horizontalRes = original.HorizontalResolution / scaleFactor;
            double verticalRes = original.VerticalResolution / scaleFactor;

            var result = new Bitmap((int)realWidth, (int)realHeight, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, (int)realWidth, (int)realHeight);
                result.SafeSetResolution((float)horizontalRes, (float)verticalRes);
                return result;
            }
        }
    }
}