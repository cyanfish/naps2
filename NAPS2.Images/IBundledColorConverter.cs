namespace NAPS2.Images;

/// <summary>
/// Applies embedded color profiles for platforms whose decoders don't do it themselves. See
/// ImageContext.BundledColorConverter.
/// </summary>
internal interface IBundledColorConverter
{
    /// <summary>
    /// Converts the image's pixels in place from the given ICC profile to sRGB. The image is left unchanged if the
    /// profile can't be applied.
    /// </summary>
    void ConvertToSrgb(IMemoryImage image, byte[] iccProfile);
}
