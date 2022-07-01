namespace NAPS2.Images;

/// <summary>
/// A common interface to wrap around platform-specific implementations of an in-memory image
/// (e.g. System.Drawing.Bitmap for Windows Forms).
/// </summary>
public interface IMemoryImage : IImageStorage
{
    int Width { get; }

    int Height { get; }

    float HorizontalResolution { get; }

    float VerticalResolution { get; }

    void SetResolution(float xDpi, float yDpi);

    ImagePixelFormat PixelFormat { get; }

    ImageFileFormat OriginalFileFormat { get; }

    ImageLockState Lock(LockMode lockMode, out IntPtr scan0, out int stride);

    void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified);

    void Save(Stream stream, ImageFileFormat imageFormat);

    IMemoryImage Clone();
}