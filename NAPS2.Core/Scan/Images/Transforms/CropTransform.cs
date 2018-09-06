using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class CropTransform : Transform
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public int? OriginalWidth { get; set; }
        public int? OriginalHeight { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            double xScale = bitmap.Width / (double)(OriginalWidth ?? bitmap.Width),
                yScale = bitmap.Height / (double)(OriginalHeight ?? bitmap.Height);

            int width = Math.Max(bitmap.Width - (int)Math.Round((Left + Right) * xScale), 1);
            int height = Math.Max(bitmap.Height - (int)Math.Round((Top + Bottom) * yScale), 1);
            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            result.SafeSetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            using (var g = Graphics.FromImage(result))
            {
                g.Clear(Color.White);
                g.DrawImage(bitmap, new Rectangle((int)Math.Round(-Left * xScale), (int)Math.Round(-Top * yScale), bitmap.Width, bitmap.Height));
            }
            OptimizePixelFormat(bitmap, ref result);
            bitmap.Dispose();
            return result;
        }

        public override bool CanSimplify(Transform other) => other is CropTransform other2
                                                             && OriginalHeight.HasValue && OriginalWidth.HasValue
                                                             && other2.OriginalHeight.HasValue && other2.OriginalWidth.HasValue;

        public override Transform Simplify(Transform other)
        {
            var other2 = (CropTransform)other;
            double xScale = (double)(other2.OriginalWidth - other2.Left - other2.Right) / (double)OriginalWidth;
            double yScale = (double)(other2.OriginalHeight - other2.Top - other2.Bottom) / (double)OriginalHeight;
            return new CropTransform
            {
                Left = (int)Math.Round(Left * xScale) + other2.Left,
                Right = (int)Math.Round(Right * xScale) + other2.Right,
                Top = (int)Math.Round(Top * yScale) + other2.Top,
                Bottom = (int)Math.Round(Bottom * yScale) + other2.Bottom,
                OriginalHeight = other2.OriginalHeight,
                OriginalWidth = other2.OriginalWidth
            };
        }

        public override bool IsNull => Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
    }
}
