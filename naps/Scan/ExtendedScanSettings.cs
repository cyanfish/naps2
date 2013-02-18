using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    public class ExtendedScanSettings : ScanSettings, IExtendedScanSettings
    {
        public ScanScale AfterScanScale { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public bool MaxQuality { get; set; }

        public int IconID { get; set; }

        public ScanHorizontalAlign PageAlign { get; set; }

        public ScanPageSize PageSize { get; set; }

        public ScanDPI Resolution { get; set; }

        public ScanSource Source { get; set; }
    }
}
