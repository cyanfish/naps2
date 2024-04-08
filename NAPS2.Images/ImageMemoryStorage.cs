namespace NAPS2.Images;

/// <summary>
/// A special type of image storage that stores an image encoded as an in-memory PNG/JPEG/PDF stream. Normally in-memory
/// image storage should use IMemoryImage, but storing a raw stream is a useful intermediate representation for some
/// serialization use cases where we don't know yet if the image will be stored in-memory or on disk. And for PDFs
/// this is the only option for in-memory storage.
/// </summary>
internal class ImageMemoryStorage : IImageStorage
{
    public ImageMemoryStorage(MemoryStream stream, string typeHint)
    {
        // Verify we have access to the underlying buffer for performance reasons
        stream.GetBuffer();
        Stream = stream;
        TypeHint = typeHint;
    }

    public ImageMemoryStorage(byte[] data, string typeHint)
        : this(new MemoryStream(data, 0, data.Length, true, true), typeHint)
    {
    }

    public MemoryStream Stream { get; }

    public string TypeHint { get; }

    public void Dispose()
    {
    }
}