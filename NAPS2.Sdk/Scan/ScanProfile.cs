using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Wia.Native;

namespace NAPS2.Scan
{
    /// <summary>
    /// A class that stores user configuration for scanning, including device selection and other options.
    /// </summary>
    [Serializable]
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
            BlankPageCoverageThreshold = 25;
            WiaDelayBetweenScansSeconds = 2.0;
        }

        public ScanProfile Clone()
        {
            var profile = (ScanProfile) MemberwiseClone();
            if (profile.AutoSaveSettings != null)
            {
                profile.AutoSaveSettings = AutoSaveSettings.Clone();
            }
            return profile;
        }

        [XmlIgnore]
        public bool IsLocked { get; set; }

        [XmlIgnore]
        public bool IsDeviceLocked { get; set; }

        public ScanDevice Device { get; set; }

        public string DriverName { get; set; }

        public ScanProxyConfig ProxyConfig { get; set; }

        public string ProxyDriverName { get; set; }

        public string DisplayName { get; set; }

        public int IconID { get; set; }

        public bool MaxQuality { get; set; }

        public bool IsDefault { get; set; }

        public int Version { get; set; }

        public bool UseNativeUI { get; set; }

        public ScanScale AfterScanScale { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public ScanHorizontalAlign PageAlign { get; set; }

        public ScanPageSize PageSize { get; set; }

        public string CustomPageSizeName { get; set; }

        public PageDimensions CustomPageSize { get; set; }

        public ScanDpi Resolution { get; set; }

        public ScanSource PaperSource { get; set; }

        public bool EnableAutoSave { get; set; }

        public AutoSaveSettings AutoSaveSettings { get; set; }

        public int Quality { get; set; }

        public bool AutoDeskew { get; set; }

        public bool BrightnessContrastAfterScan { get; set; }

        public bool ForcePageSize { get; set; }

        public bool ForcePageSizeCrop { get; set; }

        public TwainImpl TwainImpl { get; set; }

        public bool ExcludeBlankPages { get; set; }

        public int BlankPageWhiteThreshold { get; set; }

        public int BlankPageCoverageThreshold { get; set; }

        public bool WiaOffsetWidth { get; set; }

        public bool WiaRetryOnFailure { get; set; }

        public bool WiaDelayBetweenScans { get; set; }

        public double WiaDelayBetweenScansSeconds { get; set; }

        public WiaVersion WiaVersion { get; set; }

        public bool FlipDuplexedPages { get; set; }

        public KeyValueScanOptions KeyValueOptions { get; set; }
    }

    [Serializable]
    public class ScanProxyConfig
    {
        public string Name { get; set; }

        public string Ip { get; set; }

        public int? Port { get; set; }
    }

    /// <summary>
    /// User configuration for the Auto Save feature, which saves to a file immediately after scanning.
    /// </summary>
    [Serializable]
    public class AutoSaveSettings
    {
        public AutoSaveSettings()
        {
            Separator = SaveSeparator.FilePerPage;
        }

        internal AutoSaveSettings Clone() => (AutoSaveSettings) MemberwiseClone();

        public string FilePath { get; set; }

        public bool PromptForFilePath { get; set; }

        public bool ClearImagesAfterSaving { get; set; }

        public SaveSeparator Separator { get; set; }
    }

    /// <summary>
    /// The type of TWAIN driver implementation (this option is provided for compatibility).
    /// </summary>
    public enum TwainImpl
    {
        [LocalizedDescription(typeof(SettingsResources), "TwainImpl_Default")]
        Default,
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
        [LocalizedDescription(typeof(SettingsResources), "Dpi_100")]
        Dpi100,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_150")]
        Dpi150,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_200")]
        Dpi200,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_300")]
        Dpi300,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_400")]
        Dpi400,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_600")]
        Dpi600,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_800")]
        Dpi800,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_1200")]
        Dpi1200
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
        [PageDimensions("8.5", "11", PageSizeUnit.Inch)]
        Letter,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_Legal")]
        [PageDimensions("8.5", "14", PageSizeUnit.Inch)]
        Legal,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_A5")]
        [PageDimensions("148", "210", PageSizeUnit.Millimetre)]
        A5,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_A4")]
        [PageDimensions("210", "297", PageSizeUnit.Millimetre)]
        A4,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_A3")]
        [PageDimensions("297", "420", PageSizeUnit.Millimetre)]
        A3,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_B5")]
        [PageDimensions("176", "250", PageSizeUnit.Millimetre)]
        B5,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_B4")]
        [PageDimensions("250", "353", PageSizeUnit.Millimetre)]
        B4,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_Custom")]
        Custom
    }

    /// <summary>
    /// Configuration for a particular page size.
    /// </summary>
    [Serializable]
    public class PageDimensions
    {
        public decimal Width { get; set; }

        public decimal Height { get; set; }

        public PageSizeUnit Unit { get; set; }

        public override bool Equals(Object obj)
        {
            return obj is PageDimensions pageDimens && this == pageDimens;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode() ^ Unit.GetHashCode();
        }

        public static bool operator ==(PageDimensions x, PageDimensions y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }
            return x.Width == y.Width && x.Height == y.Height && x.Unit == y.Unit;
        }

        public static bool operator !=(PageDimensions x, PageDimensions y) => !(x == y);
    }

    /// <summary>
    /// Configuration for a user-created custom page size.
    /// </summary>
    public class NamedPageSize
    {
        public string Name { get; set; }

        public PageDimensions Dimens { get; set; }
    }

    /// <summary>
    /// Helper attribute used to assign physical dimensions to the ScanPageSize enum.
    /// </summary>
    public class PageDimensionsAttribute : Attribute
    {
        public PageDimensionsAttribute(string width, string height, PageSizeUnit unit)
        {
            this.PageDimensions = new PageDimensions
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
    public enum PageSizeUnit
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
        public static decimal WidthInMm(this PageDimensions pageDimensions)
        {
            switch (pageDimensions.Unit)
            {
                case PageSizeUnit.Inch:
                    return pageDimensions.Width * 25.4m;
                case PageSizeUnit.Centimetre:
                    return pageDimensions.Width * 10;
                case PageSizeUnit.Millimetre:
                    return pageDimensions.Width;
                default:
                    throw new ArgumentException();
            }
        }

        public static decimal WidthInInches(this PageDimensions pageDimensions)
        {
            switch (pageDimensions.Unit)
            {
                case PageSizeUnit.Inch:
                    return pageDimensions.Width;
                case PageSizeUnit.Centimetre:
                    return pageDimensions.Width * 0.393701m;
                case PageSizeUnit.Millimetre:
                    return pageDimensions.Width * 0.0393701m;
                default:
                    throw new ArgumentException();
            }
        }

        public static int WidthInThousandthsOfAnInch(this PageDimensions pageDimensions)
        {
            return (int)(WidthInInches(pageDimensions) * 1000);
        }

        public static decimal HeightInMm(this PageDimensions pageDimensions)
        {
            switch (pageDimensions.Unit)
            {
                case PageSizeUnit.Inch:
                    return pageDimensions.Height * 25.4m;
                case PageSizeUnit.Centimetre:
                    return pageDimensions.Height * 10;
                case PageSizeUnit.Millimetre:
                    return pageDimensions.Height;
                default:
                    throw new ArgumentException();
            }
        }

        public static decimal HeightInInches(this PageDimensions pageDimensions)
        {
            switch (pageDimensions.Unit)
            {
                case PageSizeUnit.Inch:
                    return pageDimensions.Height;
                case PageSizeUnit.Centimetre:
                    return pageDimensions.Height * 0.393701m;
                case PageSizeUnit.Millimetre:
                    return pageDimensions.Height * 0.0393701m;
                default:
                    throw new ArgumentException();
            }
        }

        public static int HeightInThousandthsOfAnInch(this PageDimensions pageDimensions)
        {
            return (int)(HeightInInches(pageDimensions) * 1000);
        }

        public static PageDimensions PageDimensions(this Enum enumValue)
        {
            object[] attrs = enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(PageDimensionsAttribute), false);
            return attrs.Cast<PageDimensionsAttribute>().Select(x => x.PageDimensions).SingleOrDefault();
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
            object[] attrs = enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attrs.Cast<DescriptionAttribute>().Select(x => x.Description).SingleOrDefault();
        }
    }
}
