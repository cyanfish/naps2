namespace NAPS2.Images;

public record ImageSaveOptions
{
    /// <summary>
    /// The quality parameter for JPEG compression, if applicable. -1 for default.
    /// </summary>
    public int Quality { get; init; } = -1;

    /// <summary>
    /// The preferred pixel format that should be used for saving. If not specified, the image's LogicalPixelFormat
    /// will be preferred to minimize disk space used.
    /// <para/>
    /// This will not result in a loss of information, e.g. if you set this to BW1 but your image has color in it, it
    /// will have no effect. If you want to change the color information, use CopyWithPixelFormat before saving.
    /// </summary>
    public ImagePixelFormat PixelFormatHint { get; init; }
}