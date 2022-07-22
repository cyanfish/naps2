namespace NAPS2.Images.Storage;

/// <summary>
/// A special type of image storage that stores an image encoded as an in-memory PNG/JPEG stream. Normally in-memory
/// image storage should use IMemoryImage, but storing a raw stream is a useful intermediate representation for some
/// serialization use cases where we don't know yet if the image will be stored in-memory or on disk.
/// </summary>
public class ImageMemoryStorage : IImageStorage
{
    public ImageMemoryStorage(MemoryStream stream, string typeHint)
    {
        Stream = stream;
        TypeHint = typeHint;
    }

    public MemoryStream Stream { get; }

    public string TypeHint { get; }

    public void Dispose()
    {
    }
}