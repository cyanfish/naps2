using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NAPS2.Images.Wpf;

/// <summary>
/// Loads the frames of an image, applying any embedded color profiles. That's done with the bundled converter if
/// possible, as it's more accurate than WIC and skips the conversion for sRGB profiles (WIC doesn't), otherwise with WIC
/// color management.
/// </summary>
internal class WpfFrameLoader
{
    private readonly Stream _stream;
    private readonly long _position;
    private readonly Func<BitmapCreateOptions, BitmapCacheOption, BitmapDecoder> _createDecoder;
    private readonly IBundledColorConverter? _converter = ImageContext.BundledColorConverter;
    private bool _useWicColorManagement;
    private BitmapDecoder _decoder;

    public WpfFrameLoader(Stream stream, Func<BitmapCreateOptions, BitmapCacheOption, BitmapDecoder> createDecoder)
    {
        _stream = stream;
        _position = stream.Position;
        _createDecoder = createDecoder;
        _useWicColorManagement = _converter == null;
        _decoder = CreateDecoder();
    }

    public int FrameCount => _decoder.Frames.Count;

    public IMemoryImage LoadFrame(int frame)
    {
        var bitmapFrame = _decoder.Frames[frame];
        var iccProfile = _useWicColorManagement ? null : GetIccProfile(bitmapFrame);
        if (iccProfile == null)
        {
            return new WpfImage(bitmapFrame);
        }
        if (bitmapFrame.Format != PixelFormats.Cmyk32)
        {
            var image = new WpfImage(bitmapFrame);
            if (_converter!.TryConvertToSrgb(image, iccProfile))
            {
                return image;
            }
            image.Dispose();
        }
        _useWicColorManagement = true;
        _stream.Seek(_position, SeekOrigin.Begin);
        _decoder = CreateDecoder();
        return new WpfImage(_decoder.Frames[frame]);
    }

    // BitmapCacheOption.None defers decoding until a frame is copied, so CMYK frames can be detected without decoding
    // them. But WIC color management is ~3x slower without caching, so we use OnLoad for that.
    private BitmapDecoder CreateDecoder() => _useWicColorManagement
        ? _createDecoder(BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
        : _createDecoder(BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);

    private static byte[]? GetIccProfile(BitmapFrame frame)
    {
        if (frame.ColorContexts is not { Count: > 0 } colorContexts)
        {
            return null;
        }
        using var profileStream = colorContexts[0].OpenProfileStream();
        var profile = new MemoryStream();
        profileStream.CopyTo(profile);
        return profile.ToArray();
    }
}
