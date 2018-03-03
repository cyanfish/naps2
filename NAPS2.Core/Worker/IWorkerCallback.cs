using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace NAPS2.Worker
{
    [ServiceContract]
    public interface IWorkerCallback
    {
        [OperationContract]
        bool Progress(int current);

        [OperationContract(IsOneWay = true)]
        void Finish(bool success);

        event Func<int, bool> OnProgress;

        bool WaitForFinish();

        [OperationContract(IsOneWay = true)]
        void Error(byte[] serializedException);
    }
}