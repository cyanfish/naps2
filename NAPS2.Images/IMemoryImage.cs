using NAPS2.Images.Bitwise;

namespace NAPS2.Images;

/// <summary>
/// A common interface to wrap around platform-specific implementations of an in-memory image
/// (e.g. System.Drawing.Bitmap for Windows Forms).
/// </summary>
public interface IMemoryImage : IImageStorage
{
    /// <summary>
    /// Gets the image's width in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the image's height in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the image's horizontal resolution in pixels per inch.
    /// </summary>
    float HorizontalResolution { get; }

    /// <summary>
    /// Gets the image's vertical resolution in pixels per inch.
    /// </summary>
    float VerticalResolution { get; }

    /// <summary>
    /// Sets the image's horizontal and vertical resolution in pixels per inch. This has no effect for invalid (zero or
    /// negative) values.
    /// </summary>
    /// <param name="xDpi">The horizontal resolution.</param>
    /// <param name="yDpi">The vertical resolution.</param>
    void SetResolution(float xDpi, float yDpi);

    /// <summary>
    /// Gets the bits per pixel for the image's underlying binary data.  
    /// </summary>
    ImagePixelFormat PixelFormat { get; }

    // TODO: Deprecate
    ImageLockState Lock(LockMode lockMode, out IntPtr scan0, out int stride);

    /// <summary>
    /// Obtains access to the underlying binary data for the image.
    /// </summary>
    /// <param name="lockMode">The access level (read/write) needed.</param>
    /// <param name="pixelInfo">Information about the raw binary pixel data.</param>
    /// <returns>An object that, when disposed, releases the lock.</returns>
    ImageLockState Lock(LockMode lockMode, out PixelInfo pixelInfo);

    /// <summary>
    /// Gets the original image file's format (e.g. png/jpeg) if known.
    /// </summary>
    ImageFileFormat OriginalFileFormat { get; set; }

    /// <summary>
    /// Saves the image to the given file path. If the file format is unspecified, it will be inferred from the
    /// file extension if possible.
    /// </summary>
    /// <param name="path">The path to save the image file to.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="quality">The quality parameter for JPEG compression, if applicable. -1 for default.</param>
    void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, int quality = -1);

    /// <summary>
    /// Saves the image to the given stream. The file format must be specified.
    /// </summary>
    /// <param name="stream">The stream to save the image to.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="quality">The quality parameter for JPEG compression, if applicable. -1 for default.</param>
    void Save(Stream stream, ImageFileFormat imageFormat, int quality = -1);

    /// <summary>
    /// Creates a copy of the image so that one can be edited or disposed without affecting the other.
    /// </summary>
    /// <returns>The copy.</returns>
    IMemoryImage Clone();

    /// <summary>
    /// Creates a copy of the image that is fully independent of the original image (i.e. doesn't share the same source
    /// stream).
    /// </summary>
    /// <returns>The copy.</returns>
    IMemoryImage SafeClone();
}