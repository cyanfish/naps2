using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    [Serializable]
    public class HueTransform : Transform
    {
        public int HueShift { get; set; }

        public override bool CanSimplify(Transform other) => other is HueTransform;

        public override Transform Simplify(Transform other)
        {
            var other2 = (HueTransform)other;
            return new HueTransform
            {
                HueShift = (HueShift + other2.HueShift + 3000) % 2000 - 1000
            };
        }

        public override bool IsNull => HueShift == 0;
    }
}
