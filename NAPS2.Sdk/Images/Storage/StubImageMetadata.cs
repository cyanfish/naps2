using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Transforms;
using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.Images.Storage
{
    public class StubImageMetadata : IImageMetadata
    {
        public List<Transform> TransformList { get; set; } = new List<Transform>();

        public int Index { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public bool Lossless { get; set; }

        public void Commit()
        {
        }

        public bool CanSerialize => true;

        public string Serialize()
        {
            return this.ToXml(Transform.KnownTransformTypes);
        }

        public void Deserialize(string serializedData)
        {
            var other = serializedData.FromXml<StubImageMetadata>(Transform.KnownTransformTypes);
            TransformList = other.TransformList;
            Index = other.Index;
            BitDepth = other.BitDepth;
            Lossless = other.Lossless;
        }

        public void Dispose()
        {
        }
    }
}
