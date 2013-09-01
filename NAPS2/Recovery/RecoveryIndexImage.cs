using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Recovery
{
    public class RecoveryIndexImage
    {
        public string FileName { get; set; }

        public RotateFlipType Transform { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public bool HighQuality { get; set; }
    }
}