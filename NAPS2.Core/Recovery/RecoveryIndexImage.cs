using NAPS2.Scan;
using NAPS2.Scan.Images.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;

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