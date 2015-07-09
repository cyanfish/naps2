using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Images.Transforms
{
    public class CropTransform : Transform
    {
        public CropTransform()
        {
        }

        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            var result = new Bitmap(bitmap.Width - Left - Right, bitmap.Height - Top - Bottom);
            var g = Graphics.FromImage(result);
            g.DrawImage(bitmap, new Rectangle(-Left, -Top, bitmap.Width, bitmap.Height));
            return result;
        }

        public override bool CanSimplify(Transform other)
        {
            return other is CropTransform;
        }

        public override Transform Simplify(Transform other)
        {
            var other2 = (CropTransform)other;
            return new CropTransform
            {
                Left = Left + other2.Left,
                Right = Right + other2.Right,
                Top = Top + other2.Top,
                Bottom = Bottom + other2.Bottom
            };
        }

        public override bool IsNull
        {
            get { return Left == 0 && Right == 0 && Top == 0 && Bottom == 0; }
        }
    }
}
