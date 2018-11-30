using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Transforms
{
    public class BlackWhiteTransform : Transform
    {
        public BlackWhiteTransform()
        {
        }

        public BlackWhiteTransform(int threshold)
        {
            Threshold = threshold;
        }
        
        public int Threshold { get; }
    }
}
