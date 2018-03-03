using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Util;

namespace NAPS2.Worker
{
    [ServiceContract]
    public interface IWorkerCallback
    {
        [OperationContract]
        bool Progress(int current, int max);

        [OperationContract(IsOneWay = true)]
        void Finish(bool success);

        event ProgressHandler OnProgress;

        bool WaitForFinish();

        [OperationContract(IsOneWay = true)]
        void Error(byte[] serializedException);
    }
}