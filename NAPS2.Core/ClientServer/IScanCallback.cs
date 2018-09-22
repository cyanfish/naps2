using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;

namespace NAPS2.ClientServer
{
    [ServiceContract]
    public interface IScanCallback
    {
        [OperationContract(IsOneWay = true)]
        void ImageReceived(byte[] imageData, RecoveryIndexImage indexImage);

        event Action<byte[], RecoveryIndexImage> ImageCallback;
    }
}
