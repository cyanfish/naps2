using NAPS2.Ocr;

namespace NAPS2.Scan;

// TODO: We can probably make this an immutable record
public class ScanOptions
{
    public Driver Driver { get; set; }

    public ScanDevice? Device { get; set; }

    public PaperSource PaperSource { get; set; }

    public int Dpi { get; set; }

    public int ScaleRatio { get; set; }

    public PageSize? PageSize { get; set; }

    // TODO: Use this as threshold for B/W scans
    public int Brightness { get; set; }

    public int Contrast { get; set; }

    public NetworkOptions NetworkOptions { get; set; } = new();

    public WiaOptions WiaOptions { get; set; } = new();

    public TwainOptions TwainOptions { get; set; } = new();

    public SaneOptions SaneOptions { get; set; } = new();

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

    public int BlankPageWhiteThreshold { get; set; }

    public int BlankPageCoverageThreshold { get; set; }

    public bool MaxQuality { get; set; }

    public int Quality { get; set; }

    public int? ThumbnailSize { get; set; }

    public bool AutoDeskew { get; set; }

    public bool FlipDuplexedPages { get; set; }
}

public enum HorizontalAlign
{
    Right,
    Center,
    Left
}