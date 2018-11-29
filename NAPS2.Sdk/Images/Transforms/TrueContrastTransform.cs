using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    public class TrueContrastTransform : Transform
    {
        public int Contrast { get; set; }

        public override bool IsNull => Contrast == 0;
    }
}
