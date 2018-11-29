using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    [Serializable]
    public class ScaleTransform : Transform
    {
        public double ScaleFactor { get; set; }

        public override bool IsNull => ScaleFactor == 1;
    }
}
