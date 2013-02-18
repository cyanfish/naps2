using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
{
    public class ScanSettings : IScanSettings
    {
        public IScanDevice Device { get; set; }

        public string DisplayName { get; set; }
    }
}
