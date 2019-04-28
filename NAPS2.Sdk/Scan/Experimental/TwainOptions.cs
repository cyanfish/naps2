using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public class TwainOptions
    {
        public TwainDsm Dsm { get; set; }

        public TwainAdapter Adapter { get; set; }

        public TwainTransferMode TransferMode { get; set; }
    }

    public enum TwainAdapter
    {
        NTwain,
        Legacy
    }

    public enum TwainDsm
    {
        New,
        NewX64,
        Old
    }

    public enum TwainTransferMode
    {
        Native,
        Memory
    }
}