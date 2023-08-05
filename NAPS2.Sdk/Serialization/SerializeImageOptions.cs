namespace NAPS2.Serialization;

internal class SerializeImageOptions
{
    /// <summary>
    /// Indicates that, when the serialized image is transferred with file-based storage, the file should be considered
    /// owned by the deserializer and the serializer will not delete or use it again. With this set, the ProcessedImage
    /// object is disposed upon serialization. Serialization will fail if there are multiple ProcessedImage objects
    /// referencing the same underlying storage.
    ///
    /// This is set when a worker is transferring a scanned/imported image to another process. 
    /// </summary>
    public bool TransferOwnership { get; init; }
    
    // TODO: Add doc/tests
    public bool ReturnOwnership { get; init; }

    /// <summary>
    /// Indicates that the image thumbnail (if present and up to date) should be transferred too. 
    /// </summary>
    public bool IncludeThumbnail { get; init; }

    /// <summary>
    /// Indicates that the image should be transferred with file-based storage. This acts as a precondition, and if set,
    /// serialization will fail if the image doesn't use file-based storage.
    ///
    /// This should be set for performance-critical situations where memory-based transfer is expected to be too slow.
    /// </summary>
    public bool RequireFileStorage { get; init; }

    /// <summary>
    /// Indicates that the image may be transferred across devices and so any file-based storage should be materialized
    /// during serialization.
    /// </summary>
    public bool CrossDevice { get; init; }

    /// <summary>
    /// Indicates a path to a image file of the rendered image. This is stored directly in the serialized proto and is
    /// used to populate PostProcessingContext for use in post-scan OCR. It doesn't affect deserialization itself.
    /// </summary>
    public string? RenderedFilePath { get; init; }
}