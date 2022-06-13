using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.Scan;

public class ScanOptions
{
    public Driver Driver { get; set; }

    public ScanDevice Device { get; set; }

    public PaperSource PaperSource { get; set; }

    public int Dpi { get; set; }

    public int ScaleRatio { get; set; }

    public PageSize PageSize { get; set; }

    public int Brightness { get; set; }

    public int Contrast { get; set; }

    public NetworkOptions NetworkOptions { get; set; } = new NetworkOptions();

    public WiaOptions WiaOptions { get; set; } = new WiaOptions();

    public TwainOptions TwainOptions { get; set; } = new TwainOptions();

    public SaneOptions SaneOptions { get; set; } = new SaneOptions();

    public BarcodeDetectionOptions BarcodeDetectionOptions { get; set; } = new BarcodeDetectionOptions();

    public BitDepth BitDepth { get; set; }

    public HorizontalAlign PageAlign { get; set; }

    public bool BrightnessContrastAfterScan { get; set; }

    public bool UseNativeUI { get; set; }

    public IntPtr DialogParent { get; set; }

    public bool NoUI { get; set; }

    public bool Modal { get; set; }

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