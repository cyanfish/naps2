using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    public class SharpenTransform : Transform
    {
        public SharpenTransform()
        {
        }

        public SharpenTransform(int sharpness)
        {
            Sharpness = sharpness;
        }

        public int Sharpness { get; }

        public override bool IsNull => Sharpness == 0;
    }
}
