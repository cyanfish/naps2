namespace NAPS2.Images;

public interface IImage : IStorage
{
    int Width { get; }

    int Height { get; }

    float HorizontalResolution { get; }

    float VerticalResolution { get; }

    void SetResolution(float xDpi, float yDpi);

    ImagePixelFormat PixelFormat { get; }

    ImageFileFormat OriginalFileFormat { get; }

    ImageLockState Lock(LockMode lockMode, out IntPtr scan0, out int stride);

    void Unlock(ImageLockState state);

    void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified);

    void Save(Stream stream, ImageFileFormat imageFormat);

    IImage Clone();
}