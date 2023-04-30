using NAPS2.Serialization;

namespace NAPS2.Scan.Internal;

// TODO: Add tests for this and/or scanperformer
internal class ScanOptionsValidator
{
    public ScanOptions ValidateAll(ScanOptions options, ScanningContext scanningContext, bool requireDevice)
    {
        // Easy deep copy. Ideally we'd do this in a more efficient way.
        options = options.ToXml().FromXml<ScanOptions>();

        options.Driver = ValidateDriver(options.Driver);
        if (options.Driver == Driver.Sane)
        {
            options.UseNativeUI = false;
        }

        if (requireDevice)
        {
            if (string.IsNullOrEmpty(options.Device?.ID))
            {
                throw new ArgumentException("ScanOptions.Device.ID must be specified");
            }
        }

        if (options.PageSize == null)
        {
            options.PageSize = PageSize.Letter;
        }
        if (options.Dpi == 0)
        {
            options.Dpi = 100;
        }
        if (options.Dpi < 0)
        {
            throw new ArgumentException("Invalid value for ScanOptions.Dpi.");
        }
        if (options.Brightness != options.Brightness.Clamp(-1000, 1000))
        {
            throw new ArgumentException("Invalid value for ScanOptions.Brightness.");
        }
        if (options.Contrast != options.Contrast.Clamp(-1000, 1000))
        {
            throw new ArgumentException("Invalid value for ScanOptions.Contrast.");
        }
        if (options.ScaleRatio == 0)
        {
            options.ScaleRatio = 1;
        }
        if (options.ScaleRatio < 0)
        {
            throw new ArgumentException("Invalid value for ScanOptions.ScaleRatio.");
        }

        if (!string.IsNullOrEmpty(options.OcrParams.LanguageCode) && scanningContext.OcrEngine == null)
        {
            throw new ArgumentException("OCR is enabled but no OCR engine is set on ScanningContext.");
        }

        // TODO: Do we need to validate the presence of a device?
        // TODO: Probably more things as well.

        return options;
    }

    public Driver ValidateDriver(Driver driver) =>
        driver == Driver.Default
            ? SystemDefaultDriver
            : driver;

    public static Driver SystemDefaultDriver
    {
        get
        {
            if (PlatformCompat.System.IsWiaDriverSupported)
            {
                return Driver.Wia;
            }
            if (PlatformCompat.System.IsAppleDriverSupported)
            {
                return Driver.Apple;
            }
            if (PlatformCompat.System.IsSaneDriverSupported)
            {
                return Driver.Sane;
            }
            return Driver.Escl;
        }
    }
}