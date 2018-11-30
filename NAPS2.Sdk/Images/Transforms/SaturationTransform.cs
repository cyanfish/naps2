using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    public class SaturationTransform : Transform
    {
        public SaturationTransform()
        {
        }

        public SaturationTransform(int saturation)
        {
            Saturation = saturation;
        }

        public int Saturation { get; }

        public override bool IsNull => Saturation == 0;
    }
}
