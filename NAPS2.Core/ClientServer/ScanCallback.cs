using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Recovery;
using NAPS2.Scan;

namespace NAPS2.ClientServer
{
    public class ScanCallback : IScanCallback
    {
        public void ImageReceived(byte[] imageData, RecoveryIndexImage indexImage)
        {
            ImageCallback?.Invoke(imageData, indexImage);
        }

        public event Action<byte[], RecoveryIndexImage> ImageCallback;
    }
}