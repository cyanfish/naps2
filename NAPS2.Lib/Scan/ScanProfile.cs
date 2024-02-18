using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using NAPS2.ImportExport;
using NAPS2.Serialization;

namespace NAPS2.Scan;

/// <summary>
/// A class that stores user configuration for scanning, including device selection and other options.
/// </summary>
public class ScanProfile
{
    public const int CURRENT_VERSION = 2;

    public ScanProfile()
    {
        // Set defaults
        BitDepth = ScanBitDepth.C24Bit;
        PageAlign = ScanHorizontalAlign.Right;
        PageSize = ScanPageSize.Letter;
        Resolution = ScanDpi.Dpi200;
        PaperSource = ScanSource.Glass;
        Quality = 75;
        BlankPageWhiteThreshold = 70;
        BlankPageCoverageThreshold = 15;
        WiaDelayBetweenScansSeconds = 2.0;
    }

    public ScanProfile Clone()
    {
        // Easy deep copy. Ideally we'd do this in a more efficient way.
        var copy = this.ToXml().FromXml<ScanProfile>();
        // Copy XmlIgnore properties
        copy.UpgradedFrom = UpgradedFrom;
        copy.IsLocked = IsLocked;
        copy.IsDeviceLocked = IsDeviceLocked;
        return copy;
    }

    public override string ToString() => DisplayName;

    public int? Version { get; set; }

    [XmlIgnore]
    public int? UpgradedFrom { get; set; }

    // TODO: These shouldn't be part of this class
    [XmlIgnore]
    public bool IsLocked { get; set; }

    [XmlIgnore]
    public bool IsDeviceLocked { get; set; }

    public ScanProfileDevice? Device { get; set; }

    public string? DriverName { get; set; }

    public string DisplayName { get; set; } = "";

    public int IconID { get; set; }

    public bool MaxQuality { get; set; }

    public bool IsDefault { get; set; }

    public bool UseNativeUI { get; set; }

    public ScanScale AfterScanScale { get; set; }

    public int Brightness { get; set; }

    public int Contrast { get; set; }

    public ScanBitDepth BitDepth { get; set; }

    public ScanHorizontalAlign PageAlign { get; set; }

    public ScanPageSize PageSize { get; set; }

    public string? CustomPageSizeName { get; set; }

    public PageDimensions? CustomPageSize { get; set; }

    public ScanDpi Resolution { get; set; }

    public ScanSource PaperSource { get; set; }

    public bool EnableAutoSave { get; set; }

    public AutoSaveSettings? AutoSaveSettings { get; set; }

    public int Quality { get; set; }

    public bool AutoDeskew { get; set; }

    public double RotateDegrees { get; set; }

    public bool BrightnessContrastAfterScan { get; set; }

    public bool ForcePageSize { get; set; }

    public bool ForcePageSizeCrop { get; set; }

    public TwainImpl TwainImpl { get; set; }

    public bool TwainProgress { get; set; }

    public bool ExcludeBlankPages { get; set; }

    public int BlankPageWhiteThreshold { get; set; }

    public int BlankPageCoverageThreshold { get; set; }

    public bool WiaOffsetWidth { get; set; }

    public bool WiaRetryOnFailure { get; set; }

    public bool WiaDelayBetweenScans { get; set; }

    public double WiaDelayBetweenScansSeconds { get; set; }

    public WiaApiVersion WiaVersion { get; set; }

    public bool FlipDuplexedPages { get; set; }

    public KeyValueScanOptions? KeyValueOptions { get; set; }
}

/// <summary>
/// User configuration for the Auto Save feature, which saves to a file immediately after scanning.
/// </summary>
public record AutoSaveSettings
{
    public string FilePath { get; init; } = "";
    public bool PromptForFilePath { get; init; }
    public bool ClearImagesAfterSaving { get; init; }
    public SaveSeparator Separator { get; init; } = SaveSeparator.FilePerPage;
}

/// <summary>
/// The type of TWAIN driver implementation (this option is provided for compatibility).
/// </summary>
public enum TwainImpl
{
    // The default is currently equivalent ot MemXfer
    [LocalizedDescription(typeof(SettingsResources), "TwainImpl_Default")]
    Default,
    [LocalizedDescription(typeof(SettingsResources), "TwainImpl_NativeXfer")]
    NativeXfer,
    [LocalizedDescription(typeof(SettingsResources), "TwainImpl_MemXfer")]
    MemXfer,
    [LocalizedDescription(typeof(SettingsResources), "TwainImpl_OldDsm")]
    OldDsm,
    [LocalizedDescription(typeof(SettingsResources), "TwainImpl_Legacy")]
    Legacy,
    [LocalizedDescription(typeof(SettingsResources), "TwainImpl_X64")]
    X64
}

/// <summary>
/// The physical source of the scanned image (flatbed, feeder).
/// </summary>
public enum ScanSource
{
    [LocalizedDescription(typeof(SettingsResources), "Source_Glass")]
    Glass,
    [LocalizedDescription(typeof(SettingsResources), "Source_Feeder")]
    Feeder,
    [LocalizedDescription(typeof(SettingsResources), "Source_Duplex")]
    Duplex
}

/// <summary>
/// The color depth used for scanning.
/// </summary>
public enum ScanBitDepth
{
    [LocalizedDescription(typeof(SettingsResources), "BitDepth_24Color")]
    C24Bit,
    [LocalizedDescription(typeof(SettingsResources), "BitDepth_8Grayscale")]
    Grayscale,
    [LocalizedDescription(typeof(SettingsResources), "BitDepth_1BlackAndWhite")]
    BlackWhite
}

/// <summary>
/// The resolution used for scanning.
/// </summary>
public enum ScanDpi
{
    Dpi100,
    Dpi150,
    Dpi200,
    Dpi300,
    Dpi400,
    Dpi600,
    Dpi800,
    Dpi1200,
    Dpi2400,
    Dpi4800
}

/// <summary>
/// The physical location of the page relative to the scan area.
/// </summary>
public enum ScanHorizontalAlign
{
    [LocalizedDescription(typeof(SettingsResources), "HorizontalAlign_Left")]
    Left,
    [LocalizedDescription(typeof(SettingsResources), "HorizontalAlign_Center")]
    Center,
    [LocalizedDescription(typeof(SettingsResources), "HorizontalAlign_Right")]
    Right
}

/// <summary>
/// A scale factor used to shrink the scanned image.
/// </summary>
public enum ScanScale
{
    [LocalizedDescription(typeof(SettingsResources), "Scale_1_1")]
    OneToOne,
    [LocalizedDescription(typeof(SettingsResources), "Scale_1_2")]
    OneToTwo,
    [LocalizedDescription(typeof(SettingsResources), "Scale_1_4")]
    OneToFour,
    [LocalizedDescription(typeof(SettingsResources), "Scale_1_8")]
    OneToEight
}

/// <summary>
/// The page size used for scanning.
/// </summary>
public enum ScanPageSize
{
    [LocalizedDescription(typeof(SettingsResources), "PageSize_Letter")]
    [PageDimensions("8.5", "11", LocalizedPageSizeUnit.Inch)]
    Letter,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_Legal")]
    [PageDimensions("8.5", "14", LocalizedPageSizeUnit.Inch)]
    Legal,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_A5")]
    [PageDimensions("148", "210", LocalizedPageSizeUnit.Millimetre)]
    A5,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_A4")]
    [PageDimensions("210", "297", LocalizedPageSizeUnit.Millimetre)]
    A4,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_A3")]
    [PageDimensions("297", "420", LocalizedPageSizeUnit.Millimetre)]
    A3,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_B5")]
    [PageDimensions("176", "250", LocalizedPageSizeUnit.Millimetre)]
    B5,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_B4")]
    [PageDimensions("250", "353", LocalizedPageSizeUnit.Millimetre)]
    B4,
    [LocalizedDescription(typeof(SettingsResources), "PageSize_Custom")]
    Custom
}

/// <summary>
/// Configuration for a particular page size.
/// </summary>
public record PageDimensions
{
    public decimal Width { get; init; }
    public decimal Height { get; init; }
    public LocalizedPageSizeUnit Unit { get; init; }
}

/// <summary>
/// Configuration for a user-created custom page size.
/// </summary>
public record NamedPageSize
{
    public string Name { get; init; } = "";
    public PageDimensions Dimens { get; init; } = new();
}

/// <summary>
/// Helper attribute used to assign physical dimensions to the ScanPageSize enum.
/// </summary>
public class PageDimensionsAttribute : Attribute
{
    public PageDimensionsAttribute(string width, string height, LocalizedPageSizeUnit unit)
    {
        PageDimensions = new PageDimensions
        {
            Width = decimal.Parse(width, CultureInfo.InvariantCulture),
            Height = decimal.Parse(height, CultureInfo.InvariantCulture),
            Unit = unit
        };
    }

    public PageDimensions PageDimensions { get; }
}

/// <summary>
/// The unit used for Width and Height in PageDimensions.
/// </summary>
public enum LocalizedPageSizeUnit
{
    [LocalizedDescription(typeof(SettingsResources), "PageSizeUnit_Inch")]
    Inch,
    [LocalizedDescription(typeof(SettingsResources), "PageSizeUnit_Centimetre")]
    Centimetre,
    [LocalizedDescription(typeof(SettingsResources), "PageSizeUnit_Millimetre")]
    Millimetre
}

/// <summary>
/// Helper extensions that get additional information from scan-related objects and enumerations.
/// </summary>
public static class ScanEnumExtensions
{
    public static PageDimensions? PageDimensions(this Enum enumValue)
    {
        var attrs = enumValue.GetType().GetField(enumValue.ToString())!.GetCustomAttributes<PageDimensionsAttribute>();
        return attrs.Select(x => x.PageDimensions).SingleOrDefault();
    }

    public static int ToIntDpi(this ScanDpi enumValue)
    {
        switch (enumValue)
        {
            case ScanDpi.Dpi100:
                return 100;
            case ScanDpi.Dpi150:
                return 150;
            case ScanDpi.Dpi200:
                return 200;
            case ScanDpi.Dpi300:
                return 300;
            case ScanDpi.Dpi400:
                return 400;
            case ScanDpi.Dpi600:
                return 600;
            case ScanDpi.Dpi800:
                return 800;
            case ScanDpi.Dpi1200:
                return 1200;
            case ScanDpi.Dpi2400:
                return 2400;
            case ScanDpi.Dpi4800:
                return 4800;
            default:
                throw new ArgumentException();
        }
    }

    public static int ToIntScaleFactor(this ScanScale enumValue)
    {
        switch (enumValue)
        {
            case ScanScale.OneToOne:
                return 1;
            case ScanScale.OneToTwo:
                return 2;
            case ScanScale.OneToFour:
                return 4;
            case ScanScale.OneToEight:
                return 8;
            default:
                throw new ArgumentException();
        }
    }

    public static string Description(this Enum enumValue)
    {
        object[] attrs =
            enumValue.GetType().GetField(enumValue.ToString())!.GetCustomAttributes(typeof(DescriptionAttribute),
                false);
        return attrs.Cast<DescriptionAttribute>().Select(x => x.Description).Single();
    }

    public static BitDepth ToBitDepth(this ScanBitDepth bitDepth)
    {
        switch (bitDepth)
        {
            case ScanBitDepth.C24Bit:
                return BitDepth.Color;
            case ScanBitDepth.Grayscale:
                return BitDepth.Grayscale;
            case ScanBitDepth.BlackWhite:
                return BitDepth.BlackAndWhite;
            default:
                throw new ArgumentException();
        }
    }

    public static ScanBitDepth ToScanBitDepth(this BitDepth bitDepth)
    {
        switch (bitDepth)
        {
            case BitDepth.Color:
                return ScanBitDepth.C24Bit;
            case BitDepth.Grayscale:
                return ScanBitDepth.Grayscale;
            case BitDepth.BlackAndWhite:
                return ScanBitDepth.BlackWhite;
            default:
                throw new ArgumentException();
        }
    }

    public static HorizontalAlign ToHorizontalAlign(this ScanHorizontalAlign horizontalAlign)
    {
        switch (horizontalAlign)
        {
            case ScanHorizontalAlign.Left:
                return HorizontalAlign.Left;
            case ScanHorizontalAlign.Right:
                return HorizontalAlign.Right;
            case ScanHorizontalAlign.Center:
                return HorizontalAlign.Center;
            default:
                throw new ArgumentException();
        }
    }

    public static PaperSource ToPaperSource(this ScanSource scanSource)
    {
        switch (scanSource)
        {
            case ScanSource.Glass:
                return PaperSource.Flatbed;
            case ScanSource.Feeder:
                return PaperSource.Feeder;
            case ScanSource.Duplex:
                return PaperSource.Duplex;
            default:
                throw new ArgumentException();
        }
    }
}