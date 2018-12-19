using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Recovery;
using NAPS2.Images;

namespace NAPS2.Worker
{
    public class WorkerCallback : IWorkerCallback
    {
        public event Action<ScannedImage, string> ImageCallback;

        public void TwainImageReceived(RecoveryIndexImage image, byte[] thumbnail, string tempImageFilePath)
        {
            // TODO
            //var scannedImage = new ScannedImage(image);
            //if (thumbnail != null)
            //{
            //    scannedImage.SetThumbnail(StorageManager.MemoryStorageFactory.Decode(new MemoryStream(thumbnail), ".bmp"));
            //}
            //ImageCallback?.Invoke(scannedImage, tempImageFilePath);
        }
    }
}
