using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class SaturationTransform : Transform
    {
        public int Saturation { get; set; }

        public override bool IsNull => Saturation == 0;
    }
}
