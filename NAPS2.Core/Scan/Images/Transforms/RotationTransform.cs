using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Images.Transforms
{
    public class RotationTransform : Transform
    {
        private const double TOLERANCE = 0.001;

        private readonly double angle;

        public static double NormalizeAngle(double angle)
        {
            var mod = angle % 360.0;
            if (mod < 0)
            {
                mod += 360.0;
            }
            return mod;
        }

        public RotationTransform(RotateFlipType rotateFlipType)
        {
            switch (rotateFlipType)
            {
                case RotateFlipType.Rotate90FlipNone:
                    angle = 90.0;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                    angle = 180.0;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    angle = 270.0;
                    break;
                case RotateFlipType.RotateNoneFlipNone:
                    angle = 0.0;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public RotationTransform(double angle)
        {
            this.angle = NormalizeAngle(angle);
        }

        public override Bitmap Perform(Bitmap bitmap)
        {
            if (Math.Abs(angle - 0.0) < TOLERANCE)
            {
                return bitmap;
            }
            if (Math.Abs(angle - 90.0) < TOLERANCE)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                return bitmap;
            }
            if (Math.Abs(angle - 180.0) < TOLERANCE)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                return bitmap;
            }
            if (Math.Abs(angle - 270.0) < TOLERANCE)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                return bitmap;
            }
            throw new NotImplementedException();
        }

        public override bool CanSimplify(Transform other)
        {
            return other is RotationTransform;
        }

        public override Transform Simplify(Transform other)
        {
            var other2 = (RotationTransform)other;
            return new RotationTransform(angle + other2.angle);
        }
    }
}
