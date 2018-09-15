using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.Worker
{
    [ServiceContract]
    public interface IWorkerCallback
    {
        [OperationContract(IsOneWay = true)]
        void TwainImageReceived(RecoveryIndexImage image, byte[] thumbnail, string tempImageFilePath);

        [OperationContract(IsOneWay = true)]
        void Finish();

        event Action<ScannedImage, string> ImageCallback;

        void WaitForFinish();

        [OperationContract(IsOneWay = true)]
        void Error(byte[] serializedException);
    }
}
