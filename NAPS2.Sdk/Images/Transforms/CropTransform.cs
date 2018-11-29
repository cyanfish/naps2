using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
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
