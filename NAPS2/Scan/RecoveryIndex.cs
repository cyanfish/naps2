using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2.Scan
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
