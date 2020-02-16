using System;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace NAPS2.Images.Transforms
{
    public class CropTransform : Transform
    {
        public CropTransform()
        {
        }

        public CropTransform(int left, int right, int top, int bottom, int? originalWidth = null, int? originalHeight = null)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
        }

        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Top { get; private set; }
        public int Bottom { get; private set; }

        public int? OriginalWidth { get; private set; }
        public int? OriginalHeight { get; private set; }

        public override bool CanSimplify(Transform other) => other is CropTransform other2
                                                             && OriginalHeight.HasValue && OriginalWidth.HasValue
                                                             && other2.OriginalHeight.HasValue && other2.OriginalWidth.HasValue;

        public override Transform Simplify(Transform other)
        {
            var other2 = (CropTransform)other;
            if (OriginalWidth == null || OriginalHeight == null || other2.OriginalWidth == null || other2.OriginalHeight == null)
            {
                throw new Exception("Can't simplify");
            }
            double xScale = (double)(other2.OriginalWidth - other2.Left - other2.Right) / (double)OriginalWidth;
            double yScale = (double)(other2.OriginalHeight - other2.Top - other2.Bottom) / (double)OriginalHeight;
            return new CropTransform
            (
                (int)Math.Round(Left * xScale) + other2.Left,
                (int)Math.Round(Right * xScale) + other2.Right,
                (int)Math.Round(Top * yScale) + other2.Top,
                (int)Math.Round(Bottom * yScale) + other2.Bottom,
                other2.OriginalWidth,
                other2.OriginalHeight
            );
        }

        public override bool IsNull => Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
    }
}
