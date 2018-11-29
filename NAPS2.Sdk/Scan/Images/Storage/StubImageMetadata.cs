using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images.Storage
{
    public class StubImageMetadata : IImageMetadata
    {
        public List<Transform> TransformList { get; set; }

        public int Index { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public bool Lossless { get; set; }

        public void Commit()
        {
        }

        public bool CanSerialize => true;

        public byte[] Serialize(IStorage storage) => throw new InvalidOperationException();

        public IStorage Deserialize(byte[] serializedData) => throw new InvalidOperationException();

        public void Dispose()
        {
        }
    }
}
