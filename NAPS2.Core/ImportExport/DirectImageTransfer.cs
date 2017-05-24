using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAPS2.Recovery;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport
{
    [Serializable]
    public class DirectImageTransfer
    {
        public DirectImageTransfer(IEnumerable<ScannedImage> selectedImages)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            ImageRecovery = selectedImages.Select(x => x.RecoveryIndexImage).ToArray();
            if (ImageRecovery.Length > 0)
            {
                RecoveryFolder = RecoveryImage.RecoveryFolder.FullName;
            }
        }

        public int ProcessID { get; }

        public RecoveryIndexImage[] ImageRecovery { get; }

        public string RecoveryFolder { get; }
    }
}
