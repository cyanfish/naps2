using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NAPS2.Recovery;
using NAPS2.Scan.Images;

namespace NAPS2.WinForms
{
    [Serializable]
    public class DirectImageTransfer
    {
        public DirectImageTransfer(IEnumerable<IScannedImage> selectedImages)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            ImageRecovery = selectedImages.OfType<FileBasedScannedImage>().Select(x => x.RecoveryIndexImage).ToArray();
            if (ImageRecovery.Length > 0)
            {
                RecoveryFolder = FileBasedScannedImage.RecoveryFolder.FullName;
            }
        }

        public int ProcessID { get; private set; }

        public RecoveryIndexImage[] ImageRecovery { get; private set; }

        public string RecoveryFolder { get; set; }
    }
}
