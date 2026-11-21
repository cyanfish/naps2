namespace NAPS2.Images;

/// <summary>
/// Applies ICC color profiles with skcms (via the naps2_jpeg native library).
/// </summary>
internal class IccColorConverter : IBundledColorConverter
{
    public static IccColorConverter? TryCreate() =>
        JpegNativeLibrary.TryGetInstance() is { } lib ? new IccColorConverter(lib) : null;

    private readonly JpegNativeLibrary.naps2_icc_to_srgb_delegate _iccToSrgb;

    private IccColorConverter(JpegNativeLibrary lib)
    {
        _iccToSrgb = lib.IccToSrgb;
    }

    public void ConvertToSrgb(IMemoryImage image, byte[] iccProfile)
    {
        using var lockState = image.Lock(LockMode.ReadWrite, out var data);
        if (data.invertY || data.invertColorSpace || data.premultipliedAlpha)
        {
            return;
        }
        // The 4th byte doesn't affect the conversion, so it can be padding rather than alpha
        int? pixelFormat = (data.bytesPerPixel, data.rOff, data.gOff, data.bOff) switch
        {
            (3, 0, 1, 2) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGB24,
            (3, 2, 1, 0) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_BGR24,
            (4, 0, 1, 2) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGBA32,
            (4, 2, 1, 0) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_BGRA32,
            _ => null
        };
        if (pixelFormat == null)
        {
            return;
        }
        var error = new byte[JpegNativeLibrary.ERROR_SIZE];
        _iccToSrgb(data.safePtr, data.w, data.h, data.stride, pixelFormat.Value, iccProfile,
            new UIntPtr((uint) iccProfile.Length), error, error.Length);
    }
}
