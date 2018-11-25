using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Scan.Images;

namespace NAPS2.Worker
{
    [ServiceContract]
    public interface IWorkerCallback
    {
        [OperationContract(IsOneWay = true)]
        void TwainImageReceived(RecoveryIndexImage image, byte[] thumbnail, string tempImageFilePath);
        
        event Action<ScannedImage, string> ImageCallback;
    }
}
