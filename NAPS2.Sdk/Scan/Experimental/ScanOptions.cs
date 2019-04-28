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

        public NetworkOptions NetworkOptions { get; set; }

        public WiaOptions WiaOptions { get; set; }

        public TwainOptions TwainOptions { get; set; }

        public SaneOptions SaneOptions { get; set; }
    }
}
