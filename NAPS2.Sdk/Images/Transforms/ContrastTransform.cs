using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    public class ContrastTransform : Transform
    {
        public ContrastTransform()
        {
        }

        public ContrastTransform(int contrast)
        {
            Contrast = contrast;
        }

        public int Contrast { get; }

        public override bool IsNull => Contrast == 0;
    }
}
