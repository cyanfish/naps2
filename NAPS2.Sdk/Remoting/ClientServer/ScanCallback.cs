using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Recovery;

namespace NAPS2.Remoting.ClientServer
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