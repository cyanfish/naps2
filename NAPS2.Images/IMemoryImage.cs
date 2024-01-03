using NAPS2.Images.Bitwise;

namespace NAPS2.Images;

/// <summary>
/// A common interface to wrap around platform-specific implementations of an in-memory image
/// (e.g. System.Drawing.Bitmap for Windows Forms).
/// </summary>
public interface IMemoryImage : IImageStorage
{
    /// <summary>
    /// Gets the image context used to create this image.
    /// </summary>
    ImageContext ImageContext { get; }

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
    /// Gets the color representation that this image is capable of storing.
    ///
    /// Callers shouldn't make assumptions about the actual in-memory binary format. If you need to know the actual
    /// representation, use the BitwiseImageData obtained from Lock().
    ///
    /// See also LogicalPixelFormat for the actual color content of the image.
    /// </summary>
    ImagePixelFormat PixelFormat { get; }

    /// <summary>
    /// Obtains access to the underlying binary data for the image.
    /// </summary>
    /// <param name="lockMode">The access level (read/write) needed.</param>
    /// <param name="imageData">The raw binary image data.</param>
    /// <returns>An object that, when disposed, releases the lock.</returns>
    ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData);

    // TODO: Change to Lossless or similar?
    /// <summary>
    /// Gets the original image file's format (e.g. png/jpeg) if known.
    /// </summary>
    ImageFileFormat OriginalFileFormat { get; set; }

    /// <summary>
    /// Gets the color content of the image. For example, an image might be stored in memory with PixelFormat = ARGB32,
    /// but if it's a grayscale image with no transparency, then LogicalPixelFormat = Gray8. By default this is not
    /// calculated and is set to ImagePixelFormat.Unsupported. Call IMemoryImage.UpdateLogicalPixelFormat() to ensure
    /// this is calculated.
    /// </summary>
    ImagePixelFormat LogicalPixelFormat { get; set; }

    /// <summary>
    /// Saves the image to the given file path. If the file format is unspecified, it will be inferred from the
    /// file extension if possible.
    /// </summary>
    /// <param name="path">The path to save the image file to.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="options">Options for saving, e.g. JPEG quality.</param>
    void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, ImageSaveOptions? options = null);

    /// <summary>
    /// Saves the image to the given stream. The file format must be specified.
    /// </summary>
    /// <param name="stream">The stream to save the image to.</param>
    /// <param name="imageFormat">The file format to use.</param>
    /// <param name="options">Options for saving, e.g. JPEG quality.</param>
    void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null);

    /// <summary>
    /// Creates a copy of the image so that one can be edited or disposed without affecting the other.
    /// </summary>
    /// <returns>The copy.</returns>
    IMemoryImage Clone();
}