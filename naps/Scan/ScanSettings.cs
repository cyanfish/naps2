using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    public class ScanSettings
    {
        public ScanDevice Device { get; set; }

        public string DisplayName { get; set; }

        public int IconID { get; set; }

        public bool MaxQuality { get; set; }
    }
}
