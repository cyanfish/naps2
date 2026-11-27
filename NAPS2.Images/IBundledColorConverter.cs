namespace NAPS2.Images;

/// <summary>
/// Applies embedded color profiles for platforms whose decoders don't do it themselves. See
/// ImageContext.BundledColorConverter.
/// </summary>
internal interface IBundledColorConverter
{
    /// <summary>
    /// Converts the image's pixels in place from the given ICC profile to sRGB. Returns false, leaving the image
    /// unchanged, if the profile can't be applied (e.g. a CMYK profile, or an unsupported pixel format).
    /// </summary>
    bool TryConvertToSrgb(IMemoryImage image, byte[] iccProfile);
}
