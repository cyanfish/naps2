using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Update
{
    public class UpdateInfo
    {
        public string Name { get; set; }

        public string DownloadUrl { get; set; }

        public byte[] Sha1 { get; set; }

        public byte[] Signature { get; set; }
    }
}
