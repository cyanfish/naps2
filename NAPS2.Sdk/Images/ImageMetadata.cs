namespace NAPS2.Images;

/// <summary>
/// Represents additional information about a scanned image (quality, page size).
/// </summary>
public record ImageMetadata(bool Lossless, PageSize? PageSize)
{
    /// <summary>
    /// A default set of metadata suitable for test images. Real use cases should be explicit and not use this default value.
    /// </summary>
    public static readonly ImageMetadata DefaultForTesting = new(false, null);
}
