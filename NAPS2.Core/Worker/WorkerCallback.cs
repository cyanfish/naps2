using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NAPS2.Recovery;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public class WorkerCallback : IWorkerCallback
    {
        private bool finished;
        private Exception exception;

        public event Action<ScannedImage> ImageCallback;

        public void TwainImageReceived(RecoveryIndexImage image, byte[] thumbnail)
        {
            var scannedImage = new ScannedImage(image);
            if (thumbnail != null)
            {
                scannedImage.SetThumbnail(new Bitmap(new MemoryStream(thumbnail)));
            }
            ImageCallback?.Invoke(scannedImage);
        }

        public void Finish()
        {
            lock (this)
            {
                finished = true;
                Monitor.Pulse(this);
            }
        }

        public void WaitForFinish()
        {
            lock (this)
            {
                if (!finished)
                {
                    Monitor.Wait(this);
                }
                if (exception != null)
                {
                    exception.PreserveStackTrace();
                    throw exception;
                }
            }
        }

        public void Error(byte[] serializedException)
        {
            exception = (Exception)new NetDataContractSerializer().Deserialize(new MemoryStream(serializedException));
        }
    }
}
