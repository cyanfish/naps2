using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Transforms;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Images.Storage
{
    public class StubImageMetadata : IImageMetadata
    {
        public List<Transform> TransformList { get; set; } = new List<Transform>();

        public int TransformState { get; set; }

        public int Index { get; set; }

        public BitDepth BitDepth { get; set; }

        public bool Lossless { get; set; }

        public void Commit()
        {
        }

        public bool CanSerialize => true;

        public string Serialize() => this.ToXml();

        public void Deserialize(string serializedData)
        {
            var other = serializedData.FromXml<StubImageMetadata>();
            TransformList = other.TransformList;
            Index = other.Index;
            BitDepth = other.BitDepth;
            Lossless = other.Lossless;
        }

        public IImageMetadata Clone() =>
            new StubImageMetadata
            {
                TransformList = TransformList.ToList(),
                TransformState = TransformState,
                Index = Index,
                BitDepth = BitDepth,
                Lossless = Lossless
            };

        public void Dispose()
        {
        }
    }
}
