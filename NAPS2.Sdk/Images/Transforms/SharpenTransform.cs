using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    [Serializable]
    public class SharpenTransform : Transform
    {
        public int Sharpness { get; set; }

        public override bool IsNull => Sharpness == 0;
    }
}
