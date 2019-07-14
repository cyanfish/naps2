using System;
using System.ServiceModel;
using NAPS2.Recovery;

namespace NAPS2.Remoting.ClientServer
{
    [ServiceContract]
    public interface IScanCallback
    {
        [OperationContract(IsOneWay = true)]
        void ImageReceived(byte[] imageData, RecoveryIndexImage indexImage);

        event Action<byte[], RecoveryIndexImage> ImageCallback;
    }
}
