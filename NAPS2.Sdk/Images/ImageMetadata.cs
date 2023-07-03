namespace NAPS2.Images;

public record ImageMetadata(BitDepth BitDepth, bool Lossless, PageSize? PageSize)
{
    /// <summary>
    /// A default set of metadata suitable for test images. Real use cases should be explicit and not use this default value.
    /// </summary>
    public static readonly ImageMetadata DefaultForTesting = new(BitDepth.Color, false, null);
}
