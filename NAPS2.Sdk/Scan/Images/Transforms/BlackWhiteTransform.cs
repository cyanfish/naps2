using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class BlackWhiteTransform : Transform
    {
        public int Threshold { get; set; }
    }
}
