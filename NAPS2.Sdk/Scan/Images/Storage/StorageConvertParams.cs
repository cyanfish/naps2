using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class StorageConvertParams
    {
        public bool Temporary { get; set; }

        public bool Lossless { get; set; }

        public int LossyQuality { get; set; }

        // TODO: Move bit depth out of Scan namespace?
        public ScanBitDepth BitDepth { get; set; }
    }
}
