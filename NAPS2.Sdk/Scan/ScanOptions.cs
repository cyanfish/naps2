using NAPS2.Ocr;

namespace NAPS2.Scan;

/// <summary>
/// Represents the options used for scanning.
/// </summary>
public class ScanOptions
{
    /// <summary>
    /// The driver type used for scanning. Supported drivers depend on the platform (Windows/Mac/Linux).
    /// </summary>
    public Driver Driver { get; set; }

    /// <summary>
    /// The physical device to scan with.
    /// </summary>
    public ScanDevice? Device { get; set; }

    /// <summary>
    /// For scanners with multiple ways to scan (flatbed/feeder/duplex), specifies which source to use.
    /// </summary>
    public PaperSource PaperSource { get; set; }

    /// <summary>
    /// The resolution to scan with, in dots-per-inch (DPI). Typical values include 100, 300, 600, 1200.
    /// </summary>
    public int Dpi { get; set; }

    /// <summary>
    /// A factor used to shrink the scanned image. A factor of 2 means to shrink the image by 2x.
    /// </summary>
    public int ScaleRatio { get; set; }

    /// <summary>
    /// The size of the page to be scanned.
    /// </summary>
    public PageSize? PageSize { get; set; }

    // TODO: Use this as threshold for B/W scans
    /// <summary>
    /// A brightness adjustment to be used for the scan, in the range -1000 (all black) to +1000 (all white).
    /// </summary>
    public int Brightness { get; set; }

    /// <summary>
    /// A contrast adjustment to be used for the scan, in the range -1000 (no contrast) to +1000 (max contrast).
    /// </summary>
    public int Contrast { get; set; }

    /// <summary>
    /// Options specific to the WIA driver.
    /// </summary>
    public WiaOptions WiaOptions { get; set; } = new();

    /// <summary>
    /// Options specific to the TWAIN driver.
    /// </summary>
    public TwainOptions TwainOptions { get; set; } = new();

    /// <summary>
    /// Options specific to the SANE driver.
    /// </summary>
    public SaneOptions SaneOptions { get; set; } = new();

    /// <summary>
    /// Options specific to the ESCL driver.
    /// </summary>
    public EsclOptions EsclOptions { get; set; } = new();

    /// <summary>
    /// Options for detecting barcodes during the scan.
    /// </summary>
    public BarcodeDetectionOptions BarcodeDetectionOptions { get; set; } = new();

    // TODO: Add another option (maybe default on) to wait for the OCR results and include them in postprocessingdata
    public OcrParams OcrParams { get; set; } = OcrParams.Empty;

    public OcrPriority OcrPriority { get; set; } = OcrPriority.Background;

    public BitDepth BitDepth { get; set; }

    public HorizontalAlign PageAlign { get; set; }

    // TODO: Consider removing this option and doing it by default
    public bool BrightnessContrastAfterScan { get; set; }

    public bool UseNativeUI { get; set; }

    public IntPtr DialogParent { get; set; }

    public bool StretchToPageSize { get; set; }

    public bool CropToPageSize { get; set; }

    public bool ExcludeBlankPages { get; set; }

    public int BlankPageWhiteThreshold { get; set; } = 70;

    public int BlankPageCoverageThreshold { get; set; } = 15;

    /// <summary>
    /// Whether scanned images should be stored using maximum (lossless) quality. Otherwise images are generally stored
    /// using JPEG compression.
    /// </summary>
    public bool MaxQuality { get; set; }

    /// <summary>
    /// The JPEG compression quality used for storing images. Ignored if MaxQuality is true.
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// If non-null, generates thumbnails of the specified size and provides them in the PostProcessingData of each
    /// scanned image.
    /// </summary>
    public int? ThumbnailSize { get; set; }

    /// <summary>
    /// Whether scanned images should go through automatic deskewing to straighten pages that are at a slight angle.
    /// </summary>
    public bool AutoDeskew { get; set; }

    /// <summary>
    /// Compatibility option to correct problems with some scanners. If this is true and the PaperSource is Duplex,
    /// even-numbered pages are flipped vertically.
    /// </summary>
    public bool FlipDuplexedPages { get; set; }

    public KeyValueScanOptions? KeyValueOptions { get; set; }
}