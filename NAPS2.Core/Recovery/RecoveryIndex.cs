using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Recovery
{
    public class RecoveryIndex
    {
        public const int CURRENT_VERSION = 1;

        public RecoveryIndex()
        {
            Images = new List<RecoveryIndexImage>();
        }

        public int Version { get; set; }

        public List<RecoveryIndexImage> Images { get; set; }
    }
}
