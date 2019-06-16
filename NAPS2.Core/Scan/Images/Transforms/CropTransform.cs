using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using NAPS2.Platform;
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

            int x = ((int)Math.Round(Left * xScale)).Clamp(0, bitmap.Width - 1);
            int y = ((int)Math.Round(Top * yScale)).Clamp(0, bitmap.Height - 1);
            int width = (bitmap.Width - (int)Math.Round((Left + Right) * xScale)).Clamp(1, bitmap.Width - x);
            int height = (bitmap.Height - (int)Math.Round((Top + Bottom) * yScale)).Clamp(1, bitmap.Height - y);

            var result = new Bitmap(width, height, bitmap.PixelFormat);
            result.SafeSetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                result.Palette.Entries[0] = bitmap.Palette.Entries[0];
                result.Palette.Entries[1] = bitmap.Palette.Entries[1];
            }
            UnsafeImageOps.RowWiseCopy(bitmap, result, x, y, width, height);
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
