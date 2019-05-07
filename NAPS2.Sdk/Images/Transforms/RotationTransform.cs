using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
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

        public RotationTransform()
        {
        }

        public RotationTransform(double angle)
        {
            Angle = NormalizeAngle(angle);
        }

        public double Angle { get; }

        public override bool CanSimplify(Transform other) => other is RotationTransform;

        public override Transform Simplify(Transform other)
        {
            var other2 = (RotationTransform)other;
            return new RotationTransform(Angle + other2.Angle);
        }

        public override bool IsNull => Math.Abs(Angle - 0.0) < TOLERANCE;
    }
}
