using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images.Storage;
using NAPS2.Ocr;

namespace NAPS2.Scan.Experimental
{
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

        public BitDepth BitDepth { get; set; }

        public HorizontalAlign PageAlign { get; set; }

        public bool BrightnessContrastAfterScan { get; set; }

        public bool UseNativeUI { get; set; }

        public IntPtr DialogParent { get; set; }

        public bool NoUI { get; set; }

        public bool Modal { get; set; }

        public bool DetectPatchCodes { get; set; }

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

        public bool DoOcr { get; set; }

        public OcrParams OcrParams { get; set; }

        // TODO: Do we need this? Can we generalize it?
        // TODO: Also find a better name. Background = should cancel if the image is invalidated.
        // TODO: Also try and get some tests going for OcrRequestQueue, that class is fragile.
        public bool OcrInBackground { get; set; }

        public CancellationToken OcrCancelToken { get; set; }
    }

    public enum HorizontalAlign
    {
        Right,
        Center,
        Left
    }

    public enum BitDepth
    {
        Color,
        Grayscale,
        BlackAndWhite
    }
}
