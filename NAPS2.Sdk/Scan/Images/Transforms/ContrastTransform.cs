using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class ContrastTransform : Transform
    {
        public int Contrast { get; set; }

        public override bool IsNull => Contrast == 0;
    }
}
