using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

/// <summary>
/// Loads the frames of an image, applying any embedded color profile. That's done with the bundled converter if
/// possible, as it's more accurate than GDI+ and skips the conversion for any sRGB profile (GDI+ only does for its own
/// one).
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
internal class GdiFrameLoader : IDisposable
{
    private const int ICC_PROFILE_PROPERTY_ID = 0x8773;
    private const PixelFormat CMYK_PIXEL_FORMAT = (PixelFormat) 0x200F;

    private readonly MemoryStream _stream;
    private readonly IBundledColorConverter? _converter = ImageContext.BundledColorConverter;
    private bool _useIcm;
    private Bitmap _bitmap;

    public GdiFrameLoader(MemoryStream stream)
    {
        _stream = stream;
        _useIcm = _converter == null;
        _bitmap = LoadBitmap();
    }

    public int FrameCount => _bitmap.GetFrameCount(FrameDimension.Page);

    public IMemoryImage LoadFrame(int frame)
    {
        SelectFrame(frame);
        var iccProfile = _useIcm ? null : GetIccProfile();
        if (iccProfile == null)
        {
            return new GdiImage(_bitmap).Copy();
        }
        if (_bitmap.PixelFormat != CMYK_PIXEL_FORMAT)
        {
            var image = new GdiImage(_bitmap).Copy();
            if (_converter!.TryConvertToSrgb(image, iccProfile))
            {
                return image;
            }
            image.Dispose();
        }
        _bitmap.Dispose();
        _useIcm = true;
        _stream.Seek(0, SeekOrigin.Begin);
        _bitmap = LoadBitmap();
        SelectFrame(frame);
        return new GdiImage(_bitmap).Copy();
    }

    private void SelectFrame(int frame)
    {
        if (frame > 0)
        {
            _bitmap.SelectActiveFrame(FrameDimension.Page, frame);
        }
    }

    private byte[]? GetIccProfile() =>
        _bitmap.PropertyIdList.Contains(ICC_PROFILE_PROPERTY_ID)
            ? _bitmap.GetPropertyItem(ICC_PROFILE_PROPERTY_ID)?.Value
            : null;

    private Bitmap LoadBitmap()
    {
        if (!_useIcm)
        {
            return (Bitmap) Image.FromStream(_stream, false, false);
        }
        try
        {
            return new Bitmap(_stream, useIcm: true);
        }
        catch (ArgumentException)
        {
            // GDI+ fails to load some images with invalid color profiles, which can still be loaded without them
            _stream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(_stream);
        }
    }

    public void Dispose() => _bitmap.Dispose();
}
