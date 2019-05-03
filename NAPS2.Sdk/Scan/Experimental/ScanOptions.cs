using System;
using System.Collections.Generic;
using System.Linq;

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
