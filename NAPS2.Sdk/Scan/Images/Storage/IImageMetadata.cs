using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images.Storage
{
    public interface IImageMetadata : IDisposable
    {
        List<Transform> TransformList { get; set; }

        int Index { get; set; }

        ScanBitDepth BitDepth { get; set; }

        bool Lossless { get; set; }

        void Commit();

        bool CanSerialize { get; }

        byte[] Serialize(IStorage storage);

        IStorage Deserialize(byte[] serializedData);
    }
}
