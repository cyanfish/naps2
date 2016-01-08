using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Recovery
{
    [Serializable]
    public class RecoveryIndexImage
    {
        public string FileName { get; set; }

        /// <summary>
        /// Deprecated
        /// </summary>
        public RotateFlipType Transform { get; set; }

        public bool ShouldSerializeTransform()
        {
            return false;
        }

        public List<Transform> TransformList { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public bool HighQuality { get; set; }
    }
}