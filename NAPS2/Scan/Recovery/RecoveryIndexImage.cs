using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Recovery
{
    public class RecoveryIndexImage
    {
        public string FileName { get; set; }

        public int Transform { get; set; }

        public int BitDepth { get; set; }

        public bool HighQuality { get; set; }
    }
}