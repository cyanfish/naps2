using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Images.Storage;

namespace NAPS2.Images.Transforms
{
    [Serializable]
    public class RotationTransform : Transform
    {
        public const double TOLERANCE = 0.001;

        public static double NormalizeAngle(double angle)
        {
            var mod = angle % 360.0;
            if (mod < 0)
            {
                mod += 360.0;
            }
            return mod;
        }

        public static RotationTransform Auto(IImage image)
        {
            return new RotationTransform(-image.GetSkewAngle());
        }

        private double angle;

        public RotationTransform()
        {
        }

        public RotationTransform(RotateFlipType rotateFlipType)
        {
            switch (rotateFlipType)
            {
                case RotateFlipType.Rotate90FlipNone:
                    Angle = 90.0;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                    Angle = 180.0;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    Angle = 270.0;
                    break;
                case RotateFlipType.RotateNoneFlipNone:
                    Angle = 0.0;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public RotationTransform(double angle)
        {
            Angle = angle;
        }

        public double Angle
        {
            get => angle;
            set => angle = NormalizeAngle(value);
        }

        public override bool CanSimplify(Transform other) => other is RotationTransform;

        public override Transform Simplify(Transform other)
        {
            var other2 = (RotationTransform)other;
            return new RotationTransform(Angle + other2.Angle);
        }

        public override bool IsNull => Math.Abs(Angle - 0.0) < TOLERANCE;
    }
}
