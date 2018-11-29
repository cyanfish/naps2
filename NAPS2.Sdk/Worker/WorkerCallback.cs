using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NAPS2.Recovery;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Storage;

namespace NAPS2.Worker
{
    public class WorkerCallback : IWorkerCallback
    {
        public event Action<ScannedImage, string> ImageCallback;

        public void TwainImageReceived(RecoveryIndexImage image, byte[] thumbnail, string tempImageFilePath)
        {
            var scannedImage = new ScannedImage(image);
            if (thumbnail != null)
            {
                scannedImage.SetThumbnail(StorageManager.MemoryStorageFactory.Decode(new MemoryStream(thumbnail), ".bmp"));
            }
            ImageCallback?.Invoke(scannedImage, tempImageFilePath);
        }
    }
}
