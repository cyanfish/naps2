using System.Windows.Media;
using System.Windows.Media.Imaging;
using Eto.Forms;
using Eto.Wpf;
using Eto.Wpf.Forms.Controls;
using Image = Eto.Drawing.Image;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Shows a greyed-out image when the button is disabled (as WPF doesn't do that by default).
/// </summary>
internal class WpfButtonHandler : ButtonHandler, Button.IHandler
{
    private const double DISABLED_IMAGE_OPACITY = 0.5;

    private BitmapSource? _enabledSource;
    private BitmapSource? _disabledSource;

    protected override void Initialize()
    {
        base.Initialize();
        Control.IsEnabledChanged += (_, _) => UpdateImage();
    }

    Image Button.IHandler.Image
    {
        get => Image;
        set
        {
            Image = value;
            UpdateImage();
        }
    }

    protected override void OnLogicalPixelSizeChanged()
    {
        base.OnLogicalPixelSizeChanged();
        UpdateImage();
    }

    private void UpdateImage()
    {
        if (ImagePart == null) return;
        var source = Image.ToWpf(ParentScale);
        if (Control.IsEnabled)
        {
            ImagePart.Source = source;
            return;
        }
        if (source != _enabledSource)
        {
            _enabledSource = source;
            _disabledSource = ToDisabledSource(source);
        }
        ImagePart.Source = _disabledSource;
    }

    private static BitmapSource ToDisabledSource(BitmapSource source)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int stride = converted.PixelWidth * 4;
        var pixels = new byte[stride * converted.PixelHeight];
        converted.CopyPixels(pixels, stride, 0);
        for (int i = 0; i < pixels.Length; i += 4)
        {
            var gray = (byte) Math.Round(pixels[i] * 0.114 + pixels[i + 1] * 0.587 + pixels[i + 2] * 0.299);
            pixels[i] = pixels[i + 1] = pixels[i + 2] = gray;
            pixels[i + 3] = (byte) Math.Round(pixels[i + 3] * DISABLED_IMAGE_OPACITY);
        }
        var bitmap = BitmapSource.Create(converted.PixelWidth, converted.PixelHeight, converted.DpiX, converted.DpiY,
            PixelFormats.Bgra32, null, pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }
}
