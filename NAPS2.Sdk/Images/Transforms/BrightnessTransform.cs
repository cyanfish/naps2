using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    public class BrightnessTransform : Transform
    {
        public BrightnessTransform()
        {
        }

        public BrightnessTransform(int brightness)
        {
            Brightness = brightness;
        }

        public int Brightness { get; }

        public override bool IsNull => Brightness == 0;
    }
}
