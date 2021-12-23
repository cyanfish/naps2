namespace NAPS2.Images.Storage;

public interface IImageMetadata : IDisposable
{
    List<Transform> TransformList { get; set; }

    int TransformState { get; set; }

    int Index { get; set; }

    BitDepth BitDepth { get; set; }

    bool Lossless { get; set; }

    void Commit();

    IImageMetadata Clone();
}